using System;

namespace Core.DomainModel.Organization
{
    public interface IExternalConnectionChangeLogEntry
    {
        Guid ExternalUnitUuid { get; set; }
        string Name { get; set; }
        ConnectionUpdateOrganizationUnitChangeType Type { get; }
        string Description { get; set; }
    }
}
