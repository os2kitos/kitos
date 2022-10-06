using System;

namespace Core.DomainModel.ItContract.Read
{
    public class ItContractOverviewReadModelItSystemUsage : IHasId
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; } //used for linking
        public string ItSystemUsageSystemUuid { get; set; }
        public string ItSystemUsageName { get; set; } //used as name when linking
        public bool ItSystemIsDisabled { get; set; }
        public int ParentId { get; set; }
        public virtual ItContractOverviewReadModel Parent { get; set; }
    }
}
