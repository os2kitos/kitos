using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task GET_Organizations_With_RightsHolders_Access_Returns_Empty_If_User_Does_Not_Have_RightsHoldersAccessInAnyOrganization()
        {
            //Arrange
            var email = CreateEmail();
            var userDetails = await HttpApi.CreateUserAndGetToken(email, OrganizationRole.User, TestEnvironment.DefaultOrganizationId, true);

            //Act
            var organizations = await OrganizationV2Helper.GetOrganizationsForWhichUserIsRightsHolder(userDetails.token);

            //Assert
            Assert.Empty(organizations);
        }

        [Fact]
        public async Task GET_Organizations_With_RightsHolders_Access_Returns_Organizations_Where_User_Has_RigtsHolderAccessRole()
        {
            //Arrange
            var email = CreateEmail();
            var userDetails = await HttpApi.CreateUserAndGetToken(email, OrganizationRole.User, TestEnvironment.DefaultOrganizationId, true);
            using var response1 = await HttpApi.SendAssignRoleToUserAsync(userDetails.userId, OrganizationRole.RightsHolderAccess, TestEnvironment.SecondOrganizationId);
            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
            var secondOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.SecondOrganizationId);
            var firstOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            var organizations = (await OrganizationV2Helper.GetOrganizationsForWhichUserIsRightsHolder(userDetails.token)).ToList();

            //Assert
            var organization = Assert.Single(organizations);
            Assert.Equal(secondOrgUuid, organization.Uuid);
            Assert.NotNull(organization.Name);

            //Assign another org and observe the change
            using var response2 = await HttpApi.SendAssignRoleToUserAsync(userDetails.userId, OrganizationRole.RightsHolderAccess, TestEnvironment.DefaultOrganizationId);
            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

            organizations = (await OrganizationV2Helper.GetOrganizationsForWhichUserIsRightsHolder(userDetails.token)).ToList();
            Assert.Equal(2, organizations.Count);
            Assert.Equal(new[] { firstOrgUuid, secondOrgUuid }.OrderBy(x => x), organizations.Select(x => x.Uuid).OrderBy(x => x));
        }

        private string CreateEmail()
        {
            return $"{nameof(OrganizationApiV2Test)}{DateTime.Now.Ticks}{A<Guid>():N}@kitos.dk";
        }
    }
}
