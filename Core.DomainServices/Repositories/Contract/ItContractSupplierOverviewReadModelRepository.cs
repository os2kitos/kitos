using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Repositories.Contract
{
    public class ItContractSupplierOverviewReadModelRepository(
        IGenericRepository<ItContractSupplierOverviewReadModel> repository)
        : IItContractSupplierOverviewReadModelRepository
    {
        public IQueryable<ItContractSupplierOverviewReadModel> GetByOrganizationId(int organizationId)
        {
            return repository.AsQueryable().ByOrganizationId(organizationId);
        }

        public Maybe<ItContractSupplierOverviewReadModel> GetByOrganizationAndSupplier(int organizationId, int supplierId)
        {
            return GetByOrganizationId(organizationId).FirstOrDefault(x => x.SupplierId == supplierId);
        }

        public IQueryable<ItContractSupplierOverviewReadModel> GetByContractId(int contractId)
        {
            return repository.AsQueryable().Where(x => x.ContractsAtHighestCriticality.Any(c => c.ContractId == contractId));
        }

        public ItContractSupplierOverviewReadModel Add(ItContractSupplierOverviewReadModel model)
        {
            var inserted = repository.Insert(model);
            repository.Save();
            return inserted;
        }

        public void Delete(ItContractSupplierOverviewReadModel model)
        {
            repository.DeleteWithReferencePreload(model);
            repository.Save();
        }
    }
}
