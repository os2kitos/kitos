using System.Linq;
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
            //var orgUnitTreeIds = _organizationUnitRepository.GetIdsOfSubTree(organizationId, responsibleOrganizationUnit).ToList();
            //return GetByOrganizationId(organizationId)
            //    .Where(model => model.ResponsibleOrganizationUnitId != null && orgUnitTreeIds.Contains(model.ResponsibleOrganizationUnitId.Value));
            return Enumerable.Empty<ItContractOverviewReadModel>().AsQueryable(); //TODO: Implement once we have the responsible org unit
        }
    }
}
