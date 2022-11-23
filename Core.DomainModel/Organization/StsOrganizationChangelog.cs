using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationChangeLog: Entity
    {
        public virtual StsOrganizationConnection StsOrganizationConnection { get; set; }
        public int StsOrganizationConnectionId { get; set; }

        public virtual User User { get; set; }
        public int? UserId { get; set; }
        
        public StsOrganizationChangeLogOrigin Origin { get; set; }
        public DateTime LogTime { get; set; }
        public virtual ICollection<StsOrganizationConsequenceLog> ConsequenceLogs { get; set; }
    }
}
