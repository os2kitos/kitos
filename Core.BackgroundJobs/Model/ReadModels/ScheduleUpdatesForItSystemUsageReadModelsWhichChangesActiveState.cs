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
                .Select(x => x.SourceEntity.Id)
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
