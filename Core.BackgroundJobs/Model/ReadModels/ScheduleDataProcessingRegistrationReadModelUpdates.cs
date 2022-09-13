using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    /// <summary>
    /// Based on updated dependencies, this job schedules new job
    /// </summary>
    public class ScheduleDataProcessingRegistrationReadModelUpdates : BaseContextToReadModelChangeScheduler<DataProcessingRegistrationReadModel, DataProcessingRegistration>
    {
        private readonly IDataProcessingRegistrationReadModelRepository _readModelRepository;
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;

        public ScheduleDataProcessingRegistrationReadModelUpdates(
            IPendingReadModelUpdateRepository updateRepository,
            IDataProcessingRegistrationReadModelRepository readModelRepository,
            IDataProcessingRegistrationRepository dataProcessingRegistrationRepository,
            ITransactionManager transactionManager) :
            base(StandardJobIds.ScheduleDataProcessingRegistrationReadModelUpdates, PendingReadModelUpdateSourceCategory.DataProcessingRegistration, transactionManager, updateRepository)
        {
            _readModelRepository = readModelRepository;
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
        }

        protected override int ProjectDependencyChangesToRoot(HashSet<int> alreadyScheduledIds, CancellationToken token)
        {
            var updatesExecuted = 0;
            updatesExecuted += HandleUserUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleSystemUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleOrganizationUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleBasisForTransferUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleDataResponsibleUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleOversightOptionUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleContractUpdates(token, alreadyScheduledIds);
            return updatesExecuted;
        }

        private int HandleOversightOptionUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges
            (
                token,
                alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.DataProcessingRegistration_OversightOption,
                update => _dataProcessingRegistrationRepository.GetByOversightOptionId(update.SourceId).Select(x => x.Id)
            );
        }

        private int HandleDataResponsibleUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges
            (
                token,
                alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.DataProcessingRegistration_DataResponsible,
                update => _dataProcessingRegistrationRepository.GetByDataResponsibleId(update.SourceId).Select(x => x.Id)
            );
        }

        private int HandleBasisForTransferUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges
            (
                token,
                alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.DataProcessingRegistration_BasisForTransfer,
                update => _dataProcessingRegistrationRepository.GetByBasisForTransferId(update.SourceId).Select(x => x.Id)
            );
        }

        private int HandleOrganizationUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges
            (
                token,
                alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.DataProcessingRegistration_Organization,
                update => _dataProcessingRegistrationRepository.GetByDataProcessorId(update.SourceId).Select(x => x.Id)
            );
        }

        private int HandleSystemUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges
            (
                token,
                alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.DataProcessingRegistration_ItSystem,
                update => _dataProcessingRegistrationRepository.GetBySystemId(update.SourceId).Select(x => x.Id)
            );
        }

        private int HandleContractUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges
            (
                token,
                alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.DataProcessingRegistration_ItContract,
                update => _dataProcessingRegistrationRepository.GetByContractId(update.SourceId).Select(x => x.Id)
            );
        }

        private int HandleUserUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges
            (
                token,
                alreadyScheduledIds,
                PendingReadModelUpdateSourceCategory.DataProcessingRegistration_User,
                update => _readModelRepository.GetByUserId(update.SourceId)
            );
        }
    }
}
