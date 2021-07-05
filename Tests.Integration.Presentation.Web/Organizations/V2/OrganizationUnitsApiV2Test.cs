using System;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationUnitsApiV2Test : WithAutoFixture
    {


        private async Task<(User user, string token)> CreateApiUser(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<Organization> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            return await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, null, A<OrganizationTypeKeys>(), AccessModifier.Public);
        }

        private string CreateName()
        {
            return $"{nameof(OrganizationUserApiV2Test)}{A<Guid>():N}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }
    }
}
