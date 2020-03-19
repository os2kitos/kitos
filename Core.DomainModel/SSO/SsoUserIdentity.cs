using System;

namespace Core.DomainModel.SSO
{
    public class SsoUserIdentity
    {
        //For EF
        public SsoUserIdentity()
        {
        }

        public SsoUserIdentity(Guid externalUuid, User user)
        {
            ExternalUuid = externalUuid;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public int Id { get; set; }
        public Guid ExternalUuid { get; set; }
        public virtual User User { get; set; }
    }
}
