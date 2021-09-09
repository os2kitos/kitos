using System;
using System.Linq;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Queries.Contract
{
    public class QueryBySupplierUuid : IDomainQuery<ItContract>
    {
        private readonly Guid _supplierUuid;

        public QueryBySupplierUuid(Guid supplierUuid)
        {
            _supplierUuid = supplierUuid;
        }

        public IQueryable<ItContract> Apply(IQueryable<ItContract> source)
        {
            return source.Where(contract => contract.Supplier != null && contract.Supplier.Uuid == _supplierUuid);
        }
    }
}
