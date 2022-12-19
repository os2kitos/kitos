using System;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class ExternalConnectionAddNewLogInput
    {
        public ExternalConnectionAddNewLogInput(int? responsibleUserId, ExternalOrganizationChangeLogResponsible responsibleType, DateTime logTime, IEnumerable<ExternalConnectionAddNewLogEntryInput> entries)
        {
            ResponsibleUserId = responsibleUserId;
            ResponsibleType = responsibleType;
            LogTime = logTime;
            Entries = entries;
        }

        public int? ResponsibleUserId { get; }
        public ExternalOrganizationChangeLogResponsible ResponsibleType { get; }
        public DateTime LogTime { get; }
        public IEnumerable<ExternalConnectionAddNewLogEntryInput> Entries { get; }
    }
}
