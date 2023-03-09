using System;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.System
{
    public class UsingOrganization
    {
        public Guid ItSystemUsageUuid { get; }
        public NamedEntity Organization { get; }
        public Guid OrganizationUuid { get; set; }

        public UsingOrganization(Guid usageUuid, NamedEntity organization, Guid organizationUuid)
        {
            ItSystemUsageUuid = usageUuid;
            Organization = organization;
            OrganizationUuid = organizationUuid;
        }
    }
}
