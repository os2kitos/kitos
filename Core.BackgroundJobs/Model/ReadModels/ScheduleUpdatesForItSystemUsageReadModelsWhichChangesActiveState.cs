using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    /// <summary>
    /// The purpose of this task is to:
    ///     - Query the current state of the read models for IT-System Usage and identify those who must be scheduled for an update since their Active state contains stale data
    ///
    /// Why do we need this?:
    ///     - Read models are normally updated whenever a change to it or one of its dependencies (or parents) change but if no user changes occur, the data will be stable
    ///     - Active state depends on the current time, and since read models are computed snapshots (to enable queries in the grid) we must keep them in sync using this job which is triggered daily (See Startup.cs)
    /// </summary>
    public class ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState : IAsyncBackgroundJob
    {
        private readonly IItSystemUsageOverviewReadModelRepository _readModelRepository;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;
        public string Id => StandardJobIds.ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState;

        public ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState(
            IItSystemUsageOverviewReadModelRepository readModelRepository,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl)
        {
            _readModelRepository = readModelRepository;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
            _transactionManager = transactionManager;
            _databaseControl = databaseControl;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            using var transaction = _transactionManager.Begin();

            var idsOfSourceEntitiesWhichHaveChangedState = _readModelRepository
                .GetReadModelsMustUpdateToChangeActiveState()
                .Select(x => x.SourceEntityId)
                .Distinct()
                .ToList();

            var pendingReadModelUpdates = idsOfSourceEntitiesWhichHaveChangedState
                .Select(id => PendingReadModelUpdate.Create(id, PendingReadModelUpdateSourceCategory.ItSystemUsage))
                .ToList();

            if (pendingReadModelUpdates.Any())
            {
                _pendingReadModelUpdateRepository.AddMany(pendingReadModelUpdates);

                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Scheduled {idsOfSourceEntitiesWhichHaveChangedState.Count} updates"));

        }
    }
}
