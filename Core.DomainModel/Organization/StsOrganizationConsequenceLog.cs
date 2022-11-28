﻿using System;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationConsequenceLog : Entity, IExternalConnectionChangeLogEntry
    {
        public int ChangeLogId { get; set; }
        public virtual StsOrganizationChangeLog ChangeLog { get; set; }
        public IExternalConnectionChangelog GetChangeLog()
        {
            return ChangeLog;
        }

        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public ConnectionUpdateOrganizationUnitChangeType Type { get; set; }
        public string Description { get; set; }
    }
}
