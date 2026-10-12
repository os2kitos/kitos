using System;

namespace Core.DomainModel.Archive;

public class ArchiveReference: Entity, IHasUuid
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public string Label { get; set; }
    public string Url { get; set; }

    public Guid ItSystemUsageArchiveUuid { get; set; }
    public virtual ItSystemUsageArchive ItSystemUsageArchive { get; set; }
}
