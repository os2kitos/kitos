using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Organization;

namespace Core.DomainServices.Repositories.Contract
{
    public class ItContractOverviewReadModelRepository : IItContractOverviewReadModelRepository
    {
        private readonly IGenericRepository<ItContractOverviewReadModel> _repository;
        private readonly IOrganizationUnitRepository _organizationUnitRepository;

        public ItContractOverviewReadModelRepository(IGenericRepository<ItContractOverviewReadModel> repository, IOrganizationUnitRepository organizationUnitRepository)
        {
            _repository = repository;
            _organizationUnitRepository = organizationUnitRepository;
        }

        public IQueryable<ItContractOverviewReadModel> GetByOrganizationId(int organizationId)
        {
            return _repository.AsQueryable().ByOrganizationId(organizationId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByOrganizationAndResponsibleOrganizationUnitId(int organizationId, int responsibleOrganizationUnit)
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
            return _repository.AsQueryable().FirstOrDefault(x => x.SourceEntityId == sourceId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByOrganizationUnit(int orgUnitId)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.ResponsibleOrgUnitId != null && x.ResponsibleOrgUnitId == orgUnitId);
        }

        public IQueryable<ItContractOverviewReadModel> GetByParentContract(int parentContractId)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.ParentContractId != null && x.ParentContractId == parentContractId);
        }
    }
}
