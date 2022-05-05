using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public static class ItcontractExtensions
    {
        public static ItContract WithSupplier(this ItContract contract, Organization supplier)
        {
            contract.SetSupplierOrganization(supplier);
            supplier.Supplier.Add(contract);
            return contract;
        }
    }
}
