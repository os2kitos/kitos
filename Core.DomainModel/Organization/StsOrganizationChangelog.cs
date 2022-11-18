using System;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationChangeLog: Entity
    {
        public StsOrganizationChangeLog()
        {
            ConsequenceLogs = new List<StsOrganizationConsequenceLog>();
        }

        public StsOrganizationChangeLogOrigin Origin { get; set; }
        public string Name { get; set; }
        public DateTime LogTime { get; set; }
        public ICollection<StsOrganizationConsequenceLog> ConsequenceLogs { get; set; }
    }
}
