using System;

namespace Core.DomainModel.SSO
{
    public class SsoOrganizationIdentity
    {
        //For EF
        public SsoOrganizationIdentity()
        {
        }

        public SsoOrganizationIdentity(Guid externalUuid, Organization.Organization organization)
        {
            ExternalUuid = externalUuid;
            Organization = organization ?? throw new ArgumentNullException(nameof(organization));
        }

        public int Id { get; set; }
        public Guid ExternalUuid { get; set; }
        public virtual Organization.Organization Organization { get; set; }
    }
}
