using System.Linq;
using Core.DomainModel.ItContract.Read;

namespace Core.DomainServices.Repositories.Contract
{
    public interface IItContractOverviewReadModelRepository
    {
        IQueryable<ItContractOverviewReadModel> GetByOrganizationId(int organizationId);
        IQueryable<ItContractOverviewReadModel> GetByOrganizationAndResponsibleOrganizationUnitId(int organizationId, int responsibleOrganizationUnit);
    }
}
