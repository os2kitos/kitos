using System;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewArchivePeriodReadModel
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
