using System;
using System.Collections.Generic;
using OrganizationEntity = Core.DomainModel.Organization.Organization;

namespace Core.DomainModel.ItContract.Read
{
    public class ItContractSupplierOverviewReadModel : IHasId, IOwnedByOrganization
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public virtual OrganizationEntity Organization { get; set; }
        public int SupplierId { get; set; }
        public ItContractSupplierType SupplierType { get; set; }
        public Guid SupplierUuid { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCvr { get; set; }
        public string SupplierForeignCvr { get; set; }
        public bool IsSupplierDisabled { get; set; }
        public Guid? HighestCriticalityUuid { get; set; }
        public string HighestCriticalityName { get; set; }
        public int? HighestCriticalityRank { get; set; }
        public string ContractsAtHighestCriticalityCsv { get; set; }
        public virtual ICollection<ItContractSupplierOverviewAtCriticalityContractReadModel> ContractsAtHighestCriticality { get; set; } = new List<ItContractSupplierOverviewAtCriticalityContractReadModel>();
    }
}
