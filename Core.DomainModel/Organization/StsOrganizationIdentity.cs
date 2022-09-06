using System;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationIdentity
    {
        //For EF
        public StsOrganizationIdentity()
        {
        }

        public StsOrganizationIdentity(Guid externalUuid, DomainModel.Organization.Organization organization)
        {
            ExternalUuid = externalUuid;
            Organization = organization ?? throw new ArgumentNullException(nameof(organization));
        }

        public int Id { get; set; }
        public Guid ExternalUuid { get; set; }
        public virtual DomainModel.Organization.Organization Organization { get; set; }
    }
}
