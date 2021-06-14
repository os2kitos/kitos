using Core.DomainModel;
using Core.DomainModel.Organization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Toolkit.Patterns;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class UserHelper : WithAutoFixture
    {
        public static async Task<User> CreateUserWithRoles(string email, string pwd, string salt, string encryptedPwd, int orgId, params OrganizationRole[] organizationRoles)
        {
            var user = new User()
            {
                Email = email,
                Password = encryptedPwd,
                HasApiAccess = true,
                Name = "CreatedByIntegrationTest",
                Salt = salt
            };
            DatabaseAccess.MutateEntitySet<User>(x => x.Insert(user));
            var userId = DatabaseAccess.MapFromEntitySet<User, int>(x => x.AsQueryable().Where(x => x.Email.Equals(user.Email)).Select(x => x.Id).FirstOrDefault());

            var orgRights = new List<OrganizationRight>();
            foreach (OrganizationRole role in organizationRoles)
            {
                orgRights.Add(new OrganizationRight()
                {
                    OrganizationId = orgId,
                    Role = role,
                    UserId = userId,
                    ObjectOwnerId = userId,
                    LastChangedByUserId = userId
                });
            }

            DatabaseAccess.MutateEntitySet<OrganizationRight>(x => x.AddRange(orgRights));
            return user;
        }
    }
}
