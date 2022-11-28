using System;

namespace Core.DomainModel.Organization
{
    public interface IExternalConnectionChangeLogEntry
    {
        IExternalConnectionChangelog GetChangeLog();
        Guid Uuid { get; set; }
        string Name { get; set; }
        ConnectionUpdateOrganizationUnitChangeType Type { get; }
        string Description { get; set; }
    }
}
