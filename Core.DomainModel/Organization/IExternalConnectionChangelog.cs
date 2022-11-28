using System;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public interface IExternalConnectionChangelog
    {
        User ResponsibleUser { get; }
        ExternalOrganizationChangeLogOrigin Origin { get; }

        DateTime LogTime { get; }
        IEnumerable<IExternalConnectionChangeLogEntry> GetEntries();
    }

}
