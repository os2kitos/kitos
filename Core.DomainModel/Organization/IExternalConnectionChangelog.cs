using System;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public interface IExternalConnectionChangelog
    {
        User ResponsibleUser { get; }
        ExternalOrganizationChangeLogResponsible ResponsibleType { get; }

        DateTime LogTime { get; }
        IEnumerable<IExternalConnectionChangeLogEntry> GetEntries();
    }

}
