using System;

namespace Core.DomainModel.SSO
{
    public class SsoUserIdentity
    {
        public int Id { get; set; }
        public Guid ExternalUuid { get; set; }
        public virtual User User { get; set; }
    }
}
