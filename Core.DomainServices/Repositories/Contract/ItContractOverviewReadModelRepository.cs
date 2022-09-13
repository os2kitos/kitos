using System;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries.Contract;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.System;

namespace Core.DomainServices.Repositories.Contract
{
    public class ItContractOverviewReadModelRepository : IItContractOverviewReadModelRepository
    {
        private readonly IGenericRepository<ItContractOverviewReadModel> _repository;
        private readonly IOrganizationUnitRepository _organizationUnitRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IItContractRepository _itContractRepository;

        public ItContractOverviewReadModelRepository(
            IGenericRepository<ItContractOverviewReadModel> repository,
            IOrganizationUnitRepository organizationUnitRepository,
            IItSystemRepository itSystemRepository,
            IItContractRepository itContractRepository)
        {
            _repository = repository;
            _organizationUnitRepository = organizationUnitRepository;
            _itSystemRepository = itSystemRepository;
            _itContractRepository = itContractRepository;
        }

        public IQueryable<ItContractOverviewReadModel> GetByOrganizationId(int organizationId)
        {
            return _repository.AsQueryable().ByOrganizationId(organizationId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByOrganizationAndResponsibleOrganizationUnitIncludingSubTree(int organizationId, int responsibleOrganizationUnit)
        {
            var orgUnitTreeIds = _organizationUnitRepository.GetIdsOfSubTree(organizationId, responsibleOrganizationUnit).ToList();
            return GetByOrganizationId(organizationId)
                .Where(model => model.ResponsibleOrgUnitId != null && orgUnitTreeIds.Contains(model.ResponsibleOrgUnitId.Value));
        }

        public ItContractOverviewReadModel Add(ItContractOverviewReadModel model)
        {
            var existing = GetBySourceId(model.SourceEntityId);

            if (existing.HasValue)
                throw new InvalidOperationException("Only one read model per contract entity is allowed");

            var inserted = _repository.Insert(model);
            _repository.Save();
            return inserted;
        }

        public void DeleteBySourceId(int sourceId)
        {
            var readModel = GetBySourceId(sourceId);
            if (readModel.HasValue)
            {
                //Delete read models - if any found
                _repository.DeleteWithReferencePreload(readModel.Value);
                _repository.Save();
            }
        }

        public Maybe<ItContractOverviewReadModel> GetBySourceId(int sourceId)
        {
            return BeginQuery()
                .FirstOrDefault(x => x.SourceEntityId == sourceId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByOrganizationUnit(int orgUnitId)
        {
            return BeginQuery()
                .Where(x => x.ResponsibleOrgUnitId == orgUnitId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByParentContract(int parentContractId)
        {
            return BeginQuery().Where(x => x.ParentContractId == parentContractId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByTerminationDeadlineType(int terminationDeadlineId)
        {
            return BeginQuery().Where(x => x.TerminationDeadlineId == terminationDeadlineId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByOptionExtendType(int optionExtendTypeId)
        {
            return BeginQuery().Where(x => x.OptionExtendId == optionExtendTypeId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByItSystem(int itSystemId)
        {
            var itSystem = _itSystemRepository.GetSystem(itSystemId);
            var emptyQuery = Enumerable.Empty<ItContractOverviewReadModel>().AsQueryable();
            if (itSystem == null)
            {
                return emptyQuery;
            }

            var usageIds = itSystem.Usages.Select(x => x.Id).ToList();

            if (!usageIds.Any())
            {
                return emptyQuery;
            }

            IQueryable<ItContractOverviewReadModel> query = null;

            foreach (var usageId in usageIds)
            {
                var byUsageQuery = GetByItSystemUsage(usageId);

                query = query == null ? byUsageQuery : query.Union(byUsageQuery);
            }

            return query;

        }

        public IQueryable<ItContractOverviewReadModel> GetByItSystemUsage(int systemUsageId)
        {
            //New entries are captured in this collection based on relations (relations are managed in the system usage)
            var contractIdsWhereUsageIsInRelations =
                _itContractRepository
                    .AsQueryable()
                    .Where(contract => contract.AssociatedSystemRelations.Any(x => x.FromSystemUsageId == systemUsageId || x.ToSystemUsageId == systemUsageId))
                    .Select(contract => contract.Id)
                    .ToList();

            //Updates to existing based on system relations
            var existingReadModelsBasedOnRelations =
                BeginQuery()
                    .Where(x => x.SystemRelations.Any(r => r.FromSystemUsageId == systemUsageId || r.ToSystemUsageId == systemUsageId));

            //Updates to existing based on system usages assigned to the contract
            var existingReadModels = BeginQuery().Where(x => x.ItSystemUsages.Any(usage => usage.ItSystemUsageId == systemUsageId));

            return existingReadModels
                .Union(existingReadModelsBasedOnRelations)
                .Union(BeginQuery().Where(x => contractIdsWhereUsageIsInRelations.Contains(x.SourceEntityId)));
        }

        public IQueryable<ItContractOverviewReadModel> GetByDataProcessingRegistration(int dprId)
        {
            //Potential updates which were not in the read model previously since they were not data processing agreements
            var contractIdsOfPotentialUpdates = _itContractRepository.AsQueryable()
                .Where(x => x.DataProcessingRegistrations.Any(dpr => dpr.Id == dprId && dpr.IsAgreementConcluded == YesNoIrrelevantOption.YES))
                .Select(x => x.Id)
                .ToList();

            return BeginQuery()
                .Where(x => x.DataProcessingAgreements.Any(dpr => dpr.DataProcessingRegistrationId == dprId))
                .Union(BeginQuery().Where(x => contractIdsOfPotentialUpdates.Contains(x.SourceEntityId)));
        }

        public IQueryable<ItContractOverviewReadModel> GetByUser(int userId)
        {
            return BeginQuery().Where(x =>
                x.LastEditedByUserId == userId ||
                x.RoleAssignments.Any(r => r.UserId == userId));
        }

        public IQueryable<ItContractOverviewReadModel> GetByProcurementStrategyType(int procurementStrategyTypeId)
        {
            return BeginQuery().Where(x => x.ProcurementStrategyId == procurementStrategyTypeId);

        }

        public IQueryable<ItContractOverviewReadModel> GetByPurchaseFormType(int purchaseFormTypeId)
        {
            return BeginQuery().Where(x => x.PurchaseFormId == purchaseFormTypeId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByContractTemplateType(int contractTemplateTypeId)
        {
            return BeginQuery().Where(x => x.ContractTemplateId == contractTemplateTypeId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByItContractType(int contractTypeId)
        {
            return BeginQuery().Where(x => x.ContractTypeId == contractTypeId);
        }

        public IQueryable<ItContractOverviewReadModel> GetBySupplier(int organizationId)
        {
            return BeginQuery().Where(x => x.SupplierId == organizationId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByCriticalityType(int criticalityTypeId)
        {
            return BeginQuery().Where(x => x.CriticalityId == criticalityTypeId);

        }

        public IQueryable<ItContractOverviewReadModel> GetReadModelsMustUpdateToChangeActiveState()
        {
            var now = DateTime.Now;
            var expiringReadModelIds = _repository
                .AsQueryable()
                .Transform(new QueryReadModelsWhichShouldExpire(now).Apply)
                .Select(x => x.Id);

            var activatingReadModelIds = _repository
                .AsQueryable()
                .Transform(new QueryReadModelsWhichShouldBecomeActive(now).Apply)
                .Select(x => x.Id);

            var idsOfReadModelsWhichMustUpdate = expiringReadModelIds.Concat(activatingReadModelIds).Distinct().ToList();

            return _repository.AsQueryable().ByIds(idsOfReadModelsWhichMustUpdate);
        }

        private IQueryable<ItContractOverviewReadModel> BeginQuery()
        {
            return _repository.AsQueryable();
        }
    }
}
