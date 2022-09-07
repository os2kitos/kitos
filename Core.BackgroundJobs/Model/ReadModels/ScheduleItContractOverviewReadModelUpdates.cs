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
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_TerminationDeadlineType, update => _readModelRepository.GetByTerminationDeadlineType(update.SourceId));
        }

        private int HandleOptionExtendTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_OptionExtendType, update => _readModelRepository.GetByOptionExtendType(update.SourceId));
        }

        private int HandleItSystemChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_ItSystem, update => _readModelRepository.GetByItSystem(update.SourceId));
        }

        private int HandleItSystemUsageChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_ItSystemUsage, update => _readModelRepository.GetByItSystemUsage(update.SourceId));
        }

        private int HandleDataProcessingRegistrationChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_DataProcessingRegistration, update => _readModelRepository.GetByDataProcessingRegistration(update.SourceId));
        }

        private int HandleUserChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_User, update => _readModelRepository.GetByUser(update.SourceId));
        }

        private int HandleProcurementStrategyTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_ProcurementStrategyType, update => _readModelRepository.GetByProcurementStrategyType(update.SourceId));
        }

        private int HandlePurchaseFormTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_PurchaseFormType, update => _readModelRepository.GetByPurchaseFormType(update.SourceId));
        }

        private int HandleItContractTemplateTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_ItContractTemplateType, update => _readModelRepository.GetByContractTemplateType(update.SourceId));
        }

        private int HandleItContractTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_ItContractType, update => _readModelRepository.GetByItContractType(update.SourceId));
        }

        private int HandleOrganizationChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_Organization, update => _readModelRepository.GetBySupplier(update.SourceId));
        }

        private int HandleCriticalityTypeChanges(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItContract_CriticalityType, update => _readModelRepository.GetByCriticalityType(update.SourceId));
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
