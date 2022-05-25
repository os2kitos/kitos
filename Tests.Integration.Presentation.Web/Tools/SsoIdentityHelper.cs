using System;
using System.Linq;
using Core.DomainModel.SSO;
using Core.DomainServices.Extensions;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class SsoIdentityHelper
    {
        public static void AddSsoIdentityToUser(int userId)
        {
            var ssoIdentity = new SsoUserIdentity
            {
                ExternalUuid = Guid.NewGuid()
            };

            DatabaseAccess.MutateDatabase(x =>
            {
                var user = x.Users.AsQueryable().ById(userId);
                if (user == null)
                    throw new ArgumentNullException($"User with Id: {userId} doesn't exist");

                user.SsoIdentities.Add(ssoIdentity);
                x.SaveChanges();
            });
        }
    }
}
