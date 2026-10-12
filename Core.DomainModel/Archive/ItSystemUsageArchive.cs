using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.Archive;

public class ItSystemUsageArchive: Entity, IArchiveEntity<ItSystemUsageArchiveSnapshot>, IOwnedByOrganization, ISystemModule
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public Guid SnapshotUuid { get; set; }
    public virtual ItSystemUsageArchiveSnapshot Snapshot { get; set; }

    public int OrganizationId { get; set; }
    public virtual Organization.Organization Organization { get; set; }

    public required string Note { get; set; }
    public required DateTime ArchivingDate { get; set; }
    public string ReferenceName { get; set; }

    public int ArchivedById { get; set; }
    public virtual User ArchivedByUser { get; set; }

    public virtual ICollection<ArchiveReference> ArchiveReferences { get; set; }
}
