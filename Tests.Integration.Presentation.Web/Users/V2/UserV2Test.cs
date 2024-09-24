using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using Presentation.Web.Models.API.V2.Request.User;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.Users;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Users.V2
{
    public class UserV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_Create_User()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user = new CreateUserRequestDTO
            {
                Email = CreateEmail(),
                FirstName = CreateName(),
                LastName = CreateName(),
                PhoneNumber = "11223344",
                DefaultUserStartPreference = A<DefaultUserStartPreferenceChoice>(),
                HasApiAccess = A<bool>(),
                HasStakeHolderAccess = A<bool>(),
                SendMailOnCreation = A<bool>(),
                Roles = A<IEnumerable<OrganizationRoleChoice>>()
            };

            //Act
            var response = await UsersV2Helper.CreateUser(organization.Uuid, user);

            //Assert
            AssertUserEqualsRequest(user, response);
        }

        [Fact]
        public async Task Can_Get_User_Collection_Permissions()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();

            //Act
            var response = await UsersV2Helper.GetUserCollectionPermissions(organization.Uuid);

            //Assert
            Assert.True(response.Create);
            Assert.True(response.Modify);
            Assert.True(response.Delete);
        }

        private void AssertUserEqualsRequest(CreateUserRequestDTO request, UserResponseDTO response)
        {
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(request.FirstName, response.FirstName);
            Assert.Equal(request.LastName, response.LastName);
            Assert.Equal(request.PhoneNumber, response.PhoneNumber);
            Assert.Equal(request.DefaultUserStartPreference, response.DefaultUserStartPreference);
            Assert.Equal(request.HasApiAccess, response.HasApiAccess);
            Assert.Equal(request.HasStakeHolderAccess, response.HasStakeHolderAccess);

            AssertUserRoles(request.Roles, response.Roles);
        }

        private void AssertUserRoles(IEnumerable<OrganizationRoleChoice> requestRoles, IEnumerable<OrganizationRoleChoice> responseRoles)
        {
            var requestRolesList = requestRoles.ToList();
            var responseRolesList = responseRoles.ToList();
            Assert.Equal(requestRolesList.Count, responseRolesList.Count);
            foreach (var requestRole in requestRolesList)
            {
                Assert.Contains(requestRole, responseRolesList);
            }
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "11224455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(UserV2Test)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}@kitos.dk";
        }
    }
}
