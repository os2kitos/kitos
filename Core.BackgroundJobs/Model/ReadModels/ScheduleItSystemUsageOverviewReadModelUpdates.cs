using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class ScheduleItSystemUsageOverviewReadModelUpdates : BaseContextToReadModelChangeScheduler<ItSystemUsageOverviewReadModel, ItSystemUsage>
    {
        private readonly IItSystemUsageOverviewReadModelRepository _readModelRepository;
        private readonly IItSystemUsageRepository _itSystemUsageRepository;
        private readonly IItSystemRepository _itSystemRepository;

        public ScheduleItSystemUsageOverviewReadModelUpdates(
            IPendingReadModelUpdateRepository updateRepository,
            IItSystemUsageOverviewReadModelRepository readModelRepository,
            IItSystemUsageRepository itSystemUsageRepository,
            IItSystemRepository itSystemRepository,
            ITransactionManager transactionManager) :
            base(
                StandardJobIds.ScheduleItSystemUsageOverviewReadModelUpdates,
                PendingReadModelUpdateSourceCategory.ItSystemUsage,
                transactionManager,
                updateRepository
                )
        {
            _readModelRepository = readModelRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _itSystemRepository = itSystemRepository;
        }

        protected override int ProjectDependencyChangesToRoot(HashSet<int> alreadyScheduledIds, CancellationToken token)
        {
            var updatesExecuted = 0;

            updatesExecuted += HandleSystemUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleUserUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleOrganizationUnitUpdated(token, alreadyScheduledIds);
            updatesExecuted += HandleOrganizationUpdated(token, alreadyScheduledIds);
            updatesExecuted += HandleBusinessTypeUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleTaskRefUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleContractUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleDataProcessingRegistrationUpdates(token, alreadyScheduledIds);
            updatesExecuted += HandleInterfaceUpdates(token, alreadyScheduledIds);

            return updatesExecuted;
        }

        private int HandleSystemUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItSystem,
                update =>
                {
                    return _itSystemUsageRepository
                        .GetBySystemId(update.SourceId)
                        .Union(_itSystemUsageRepository.GetByParentSystemId(update.SourceId))
                        .Union(_itSystemUsageRepository.GetBySystemIdInSystemRelations(update.SourceId))
                        .Select(x => x.Id);
                });
        }

        private int HandleUserUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_User, update => _readModelRepository.GetByUserId(update.SourceId));
        }

        private int HandleOrganizationUnitUpdated(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_OrganizationUnit, update => _readModelRepository.GetByOrganizationUnitId(update.SourceId));
        }

        private int HandleOrganizationUpdated(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_Organization, update => _readModelRepository.GetByDependentOrganizationId(update.SourceId));
        }

        private int HandleBusinessTypeUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_BusinessType, update => _readModelRepository.GetByBusinessTypeId(update.SourceId));
        }

        private int HandleTaskRefUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_TaskRef,
                update =>
                {
                    var systemIds = _itSystemRepository.GetByTaskRefId(update.SourceId).Select(x => x.Id).ToList();

                    return _itSystemUsageRepository.GetBySystemIds(systemIds).Select(x => x.Id);
                });
        }

        private int HandleContractUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_Contract, update =>
            {
                var existingReadModels = _readModelRepository.GetByContractId(update.SourceId).Select(x => x.SourceEntityId).ToList();
                var currentUsageIds = _itSystemUsageRepository.GetByContractId(update.SourceId).Select(x => x.Id).ToList();
                return existingReadModels.Concat(currentUsageIds).Distinct().ToList().AsQueryable();
            });
        }

        private int HandleDataProcessingRegistrationUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_DataProcessingRegistration,
                update =>
                {
                    var existingReadModelUsageIds = _readModelRepository.GetByDataProcessingRegistrationId(update.SourceId).Select(x => x.SourceEntityId).ToList();
                    var currentUsageIds = _itSystemUsageRepository.GetByDataProcessingAgreement(update.SourceId).Select(x => x.Id).ToList();

                    return existingReadModelUsageIds.Concat(currentUsageIds).Distinct().ToList().AsQueryable();
                });
        }

        private int HandleInterfaceUpdates(CancellationToken token, HashSet<int> alreadyScheduledIds)
        {
            return ScheduleRootEntityChanges(token, alreadyScheduledIds, PendingReadModelUpdateSourceCategory.ItSystemUsage_ItInterface, update => _readModelRepository.GetByItInterfaceId(update.SourceId));
        }
    }
}
