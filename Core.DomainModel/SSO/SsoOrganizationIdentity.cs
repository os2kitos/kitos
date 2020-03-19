using System;

namespace Core.DomainModel.SSO
{
    public class SsoOrganizationIdentity
    {
        public int Id { get; set; }
        public Guid ExternalUuid { get; set; }
        public virtual Organization.Organization Organization { get; set; }
    }
}
