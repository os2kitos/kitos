using System;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public interface IExternalConnectionChangelog
    {
        int Id { get; }
        User ResponsibleUser { get; }
        ExternalOrganizationChangeLogResponsible ResponsibleType { get; }

        DateTime LogTime { get; }
        IEnumerable<IExternalConnectionChangeLogEntry> GetEntries();
    }

}
