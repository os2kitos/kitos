using System;

namespace Core.DomainModel.SSO
{
    public class SsoUserIdentity
    {
        public int Id { get; set; }
        public Guid ExternalUuid { get; set; }
        public User User { get; set; }
    }
}
