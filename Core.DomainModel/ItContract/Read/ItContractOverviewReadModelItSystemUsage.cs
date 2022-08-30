using System;

namespace Core.DomainModel.ItContract.Read
{
    public class ItContractOverviewReadModelItSystemUsage : IHasId
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; } //used for linking
        public Guid ItSystemUsageSystemUuid { get; set; }
        public string ItSystemUsageName { get; set; } //used as name when linking
        public int ParentId { get; set; }
        public virtual ItContractOverviewReadModel Parent { get; set; }
    }
}
