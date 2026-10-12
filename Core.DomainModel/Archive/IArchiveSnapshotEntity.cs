using System;

namespace Core.DomainModel.Archive;

public interface IArchiveSnapshotEntity : IHasUuid
{
    Guid ItSystemUsageArchiveUuid { get; set; }
    ItSystemUsageArchive ItSystemUsageArchive { get; set; }
}
