using System;
using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Model.Authentication
{
    public class KitosApiToken
    {
        public User User { get; }
        public string Value { get; }
        public DateTime Expiration { get; }
        public int ActiveOrganizationId { get; }

        public KitosApiToken(User user, string value, DateTime expiration, int activeOrganizationId)
        {
            User = user;
            Value = value;
            Expiration = expiration;
            ActiveOrganizationId = activeOrganizationId;
        }
    }
}