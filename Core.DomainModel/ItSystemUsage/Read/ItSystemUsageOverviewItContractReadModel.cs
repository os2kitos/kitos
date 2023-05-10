using System;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewItContractReadModel
    {
        public int Id { get; set; }

        public Guid ItContractUuid { get; set; }
        public int ItContractId { get; set; }
        public string ItContractName { get; set; }

        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
