using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;

namespace Core.DomainServices.Repositories.Contract
{
    public interface IItContractSupplierOverviewReadModelRepository
    {
        IQueryable<ItContractSupplierOverviewReadModel> GetByOrganizationId(int organizationId);
        Maybe<ItContractSupplierOverviewReadModel> GetByOrganizationAndSupplier(int organizationId, int supplierId);
        IQueryable<ItContractSupplierOverviewReadModel> GetByContractId(int contractId);
        ItContractSupplierOverviewReadModel Add(ItContractSupplierOverviewReadModel model);
        void Delete(ItContractSupplierOverviewReadModel model);
    }
}
