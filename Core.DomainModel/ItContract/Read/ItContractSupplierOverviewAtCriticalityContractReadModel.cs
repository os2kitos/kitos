using System;

namespace Core.DomainModel.ItContract.Read
{
    public class ItContractSupplierOverviewAtCriticalityContractReadModel : IHasId
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public Guid ContractUuid { get; set; }
        public string ContractName { get; set; }
        public int ParentId { get; set; }
        public virtual ItContractSupplierOverviewReadModel Parent { get; set; }
    }
}
