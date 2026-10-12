using System;
using System.Collections.Generic;

namespace Core.DomainModel.Archive;

public interface IArchiveEntity<TSnapshot> : IHasUuid
    where TSnapshot : class, IArchiveSnapshotEntity
{
    int OrganizationId { get; set; }
    Organization.Organization Organization { get; set; }
    Guid SnapshotUuid { get; set; }
    TSnapshot Snapshot { get; set; }
    ICollection<ArchiveReference> ArchiveReferences { get; set; }
}
