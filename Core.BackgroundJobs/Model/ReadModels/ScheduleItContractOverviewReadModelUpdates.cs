using System;
using System.Collections.Generic;
using System.Threading;
using Core.DomainModel.BackgroundJobs;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class ScheduleItContractOverviewReadModelUpdates : BaseContextToReadModelChangeScheduler
    {
        private readonly IItContractOverviewReadModelRepository _readModelRepository;

        public ScheduleItContractOverviewReadModelUpdates(
            IPendingReadModelUpdateRepository updateRepository,
            IItContractOverviewReadModelRepository readModelRepository,
            IItContractRepository itContractRepository,
            ITransactionManager transactionManager):base(
            StandardJobIds.ScheduleItContractReadModelUpdates,
            PendingReadModelUpdateSourceCategory.ItContract,
            transactionManager,
            updateRepository)
        {
            _readModelRepository = readModelRepository;
        }

        protected override int ProjectDependencyChangesToRoot(HashSet<int> alreadyScheduledIds, CancellationToken token)
        {
            var updatesExecuted = 0;

            updatesExecuted += HandleParentContractUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleOrganizationUnitChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleCriticalityTypeChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleOrganizationChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleItContractTypeChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleItContractTemplateTypeChanges(token, alreadyScheduledIds);
            updatesExecuted += HandlePurchaseFormTypeChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleProcurementStrategyTypeChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleOptionExtendTypeChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleTerminationDeadlineTypeChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleUserChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleDataProcessingRegistrationChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleItSystemChanges(token, alreadyScheduledIds);
            updatesExecuted += HandleItSystemUsageChanges(token, alreadyScheduledIds);

            return updatesExecuted;
        }

        private int HandleTerminationDeadlineTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleOptionExtendTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleItSystemChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleItSystemUsageChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleDataProcessingRegistrationChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleUserChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleProcurementStrategyTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandlePurchaseFormTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleItContractTemplateTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleItContractTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleOrganizationChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleCriticalityTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            throw new NotImplementedException();
        }

        private int HandleOrganizationUnitChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_OrgaizationUnit, update => _readModelRepository.GetByOrganizationUnit(update.SourceId));
        }

        private int HandleParentContractUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_Parent, update => _readModelRepository.GetByParentContract(update.SourceId));
        }
    }
}
