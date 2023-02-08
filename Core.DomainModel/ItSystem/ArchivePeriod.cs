using System;

namespace Core.DomainModel.ItSystem
{
    public class ArchivePeriod : Entity, ISystemModule, IHasUuid
    {
        public ArchivePeriod()
        {
            Uuid = Guid.NewGuid();
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UniqueArchiveId { get; set; }
        public int ItSystemUsageId { get; set; }
        public ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }
        public bool Approved { get; set; }
        public Guid Uuid { get; set; }
    }
}
