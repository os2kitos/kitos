using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;

namespace Core.DomainModel.Organization
{
    public interface IExternalConnectionChangelog
    {
        Maybe<User> ResponsibleUser { get; }
        ExternalOrganizationChangeLogOrigin Origin { get; }

        DateTime LogTime { get; }

        IEnumerable<IExternalConnectionChangeLogEntry> Entries { get; }
    }
    public class StsOrganizationChangeLog: Entity, IExternalConnectionChangelog
    {
        public virtual StsOrganizationConnection StsOrganizationConnection { get; set; }
        public int StsOrganizationConnectionId { get; set; }

        public virtual User User { get; set; }
        public int? UserId { get; set; }
        
        public ExternalOrganizationChangeLogOrigin Origin { get; set; }
        public DateTime LogTime { get; set; }
        public virtual ICollection<StsOrganizationConsequenceLog> ConsequenceLogs { get; set; }
    }
}
