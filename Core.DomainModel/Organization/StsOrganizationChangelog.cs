﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationChangeLog: Entity, IExternalConnectionChangelog
    {
        public StsOrganizationChangeLog()
        {
            Entries = new List<StsOrganizationConsequenceLog>();
        }

        public virtual StsOrganizationConnection StsOrganizationConnection { get; set; }
        public int StsOrganizationConnectionId { get; set; }

        public virtual User ResponsibleUser { get; set; }
        public int? ResponsibleUserId { get; set; }
        
        public ExternalOrganizationChangeLogOrigin Origin { get; set; }
        public DateTime LogTime { get; set; }
        public virtual IEnumerable<IExternalConnectionChangeLogEntry> Entries { get; set; }

        public IEnumerable<IExternalConnectionChangeLogEntry> GetEntries()
        {
            return Entries.ToList();
        }
    }
}
