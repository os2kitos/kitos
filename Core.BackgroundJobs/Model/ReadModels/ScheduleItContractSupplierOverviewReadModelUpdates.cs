using System.Collections.Generic;
using System.Threading;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class ScheduleItContractSupplierOverviewReadModelUpdates(
        IPendingReadModelUpdateRepository updateRepository,
        IItContractOverviewReadModelRepository contractOverviewReadModelRepository,
        ITransactionManager transactionManager)
        : BaseContextToReadModelChangeScheduler<ItContractOverviewReadModel, ItContract>(
            StandardJobIds.ScheduleItContractSupplierOverviewReadModelUpdates,
            PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview,
            transactionManager,
            updateRepository)
    {
        protected override int ProjectDependencyChangesToRoot(HashSet<int> alreadyScheduledIds, CancellationToken token)
        {
            var updatesExecuted = 0;
            updatesExecuted += HandleCriticalityTypeChanges(alreadyScheduledIds, token);
            updatesExecuted += HandleOrganizationChanges(alreadyScheduledIds, token);
            return updatesExecuted;
        }

        private int HandleCriticalityTypeChanges(HashSet<int> alreadyScheduledIds, CancellationToken token)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview_CriticalityType,
                update => contractOverviewReadModelRepository.GetByCriticalityType(update.SourceId));
        }

        private int HandleOrganizationChanges(HashSet<int> alreadyScheduledIds, CancellationToken token)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.ItContract_SupplierOverview_Organization,
                update => contractOverviewReadModelRepository.GetBySupplier(update.SourceId));
        }
    }
}
