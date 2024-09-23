using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            var userRequest = CreateCreateUserRequest();

            //Act
            var response = await UsersV2Helper.CreateUser(organization.Uuid, userRequest);

            //Assert
            AssertUserEqualsRequest(userRequest, response);
        }

        [Fact]
        public async Task Can_Send_User_Notification()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var userRequest = CreateCreateUserRequest();
            var user = await UsersV2Helper.CreateUser(organization.Uuid, userRequest);

            //Act
            var response = await UsersV2Helper.SendNotification(organization.Uuid, user.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

        private CreateUserRequestDTO CreateCreateUserRequest()
        {
            return new CreateUserRequestDTO
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
        }
    }
}
