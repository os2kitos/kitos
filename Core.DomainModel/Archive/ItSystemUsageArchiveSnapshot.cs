using System;

namespace Core.DomainModel.Archive;

public class ItSystemUsageArchiveSnapshot: Entity, IArchiveSnapshotEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public DateTime? TakenIntoUsageDate { get; set; }
    public string LegacyName { get; set; }
    public string LocalName { get; set; }
    public string LocalId { get; set; }

    public Guid ItSystemUuid { get; set; }
    public virtual ItSystem.ItSystem ItSystem { get; set; }

    public Guid ItSystemUsageArchiveUuid { get; set; }
    public virtual ItSystemUsageArchive ItSystemUsageArchive { get; set; }
}
