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
using System;
using Presentation.Web.Models.API.V2.Internal.Request.User;

namespace Tests.Integration.Presentation.Web.Users.V2
{
    public class UserInternalApiV2Test : WithAutoFixture
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
            AssertUserEqualsCreateRequest(userRequest, response);
        }

        [Fact]
        public async Task Can_Send_User_Notification()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user = await CreateUserAsync(organization.Uuid); ;

            //Act
            var response = await UsersV2Helper.SendNotification(organization.Uuid, user.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

        [Fact]
        public async Task Can_Update_User()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user = await CreateUserAsync(organization.Uuid); ;

            //Act
            var updateRequest = A<UpdateUserRequestDTO>();
            var response = await UsersV2Helper.UpdateUser(organization.Uuid, user.Uuid, updateRequest);

            //Assert
            AssertUserEqualsUpdateRequest(updateRequest, response);
        }

        private void AssertUserEqualsCreateRequest(CreateUserRequestDTO request, UserResponseDTO response)
        {
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(request.FirstName, response.FirstName);
            Assert.Equal(request.LastName, response.LastName);
            
            AssertBaseUserRequestMatches(request, response);
        }

        [Fact]
        public async Task Can_Get_User_With_Existing_Email_In_Other_Orgs()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var organization2 = await CreateOrganizationAsync();
            var user = await CreateUserAsync(organization.Uuid); ;

            //Act
            var response = await UsersV2Helper.GetUserByEmail(organization2.Uuid, user.Email);

            //Assert
            Assert.Equal(user.Uuid, response.Uuid);
        }

        [Fact]
        public async Task Receive_Error_If_Try_To_Copy_Roles_From_User_To_Itself()
        {
            var organization = await CreateOrganizationAsync();
            var user = await CreateUserAsync(organization.Uuid);
            var response = await UsersV2Helper.CopyRoles(organization.Uuid, user.Uuid, user.Uuid,
                A<MutateUserRightsRequestDTO>());

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Can_Copy_User_Roles()
        {
            var organization = await CreateOrganizationAsync();
            var fromUser = await CreateUserAsync(organization.Uuid);
            var toUser = await CreateUserAsync(organization.Uuid);
            var request = A<MutateUserRightsRequestDTO>();
            //Act
            var result = await UsersV2Helper.CopyRoles(organization.Uuid, fromUser.Uuid, toUser.Uuid, request);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            
        }

        [Fact]
        public async Task Copy_Roles_Ignores_Existing_Roles()
        {
            var organization = await CreateOrganizationAsync();
            var fromUser = await CreateUserAsync(organization.Uuid);
            var toUser = await CreateUserAsync(organization.Uuid);
            var request = A<MutateUserRightsRequestDTO>();
            await UsersV2Helper.CopyRoles(organization.Uuid, fromUser.Uuid, toUser.Uuid, request);

            var repeatedRequestResult =
                await UsersV2Helper.CopyRoles(organization.Uuid, fromUser.Uuid, toUser.Uuid, request);

            Assert.Equal(HttpStatusCode.OK, repeatedRequestResult.StatusCode);

        }

        [Fact]
        public async Task Can_Transfer_User_Roles()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var fromUser = await CreateUserAsync(organization.Uuid);
            var toUser = await CreateUserAsync(organization.Uuid);
            var request = A<MutateUserRightsRequestDTO>();
            //Act
            var result = await UsersV2Helper.TransferRoles(organization.Uuid, fromUser.Uuid, toUser.Uuid, request);
            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_User()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var organization2 = await CreateOrganizationAsync();
            var userRequest = CreateCreateUserRequest();
            var user = await UsersV2Helper.CreateUser(organization.Uuid, userRequest);
            var userUpdateRequest = new 
            {
                Roles = new List<OrganizationRoleChoice> { OrganizationRoleChoice.User }
            };
            var userInOrg2 = await UsersV2Helper.UpdateUser(organization2.Uuid, user.Uuid, userUpdateRequest);

            Assert.Equal(user.Uuid, userInOrg2.Uuid);

            //Act
            _ = await UsersV2Helper.DeleteUserAndVerifyStatusCode(organization.Uuid, user.Uuid);

            //Assert
            var userInOrg2AfterDeletion = await UsersV2Helper.GetUserByEmail(organization2.Uuid, user.Email);
            Assert.Equal(userInOrg2.Uuid, userInOrg2AfterDeletion.Uuid);

            var deletedUser = await UsersV2Helper.GetUserByEmail(organization.Uuid, user.Email);
            Assert.False(deletedUser.IsPartOfCurrentOrganization);
        }

        [Fact]
        public async Task Can_Delete_User_In_All_Orgs()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var organization2 = await CreateOrganizationAsync();
            var userRequest = CreateCreateUserRequest();
            var user = await UsersV2Helper.CreateUser(organization.Uuid, userRequest);
            var userUpdateRequest = new 
            {
                Roles = new List<OrganizationRoleChoice> { OrganizationRoleChoice.User }
            };
            var userInOrg2 = await UsersV2Helper.UpdateUser(organization2.Uuid, user.Uuid, userUpdateRequest);

            Assert.Equal(user.Uuid, userInOrg2.Uuid);

            //Act
            await UsersV2Helper.DeleteUserGlobally(user.Uuid);

            //Assert
            var userOrg1 = await UsersV2Helper.GetUserByEmail(organization.Uuid, user.Email);
            Assert.Null(userOrg1);
            var userOrg2 = await UsersV2Helper.GetUserByEmail(organization2.Uuid, user.Email);
            Assert.Null(userOrg2);
        }

        [Fact]
        public async Task Delete_User_Returns_Not_Found_If_Invalid_Org_Uuid()
        {
            var organization = await CreateOrganizationAsync();
            var user = await UsersV2Helper.CreateUser(organization.Uuid, CreateCreateUserRequest());
            var invalidOrgUuid = new Guid();

            _ = await UsersV2Helper.DeleteUserAndVerifyStatusCode(invalidOrgUuid, user.Uuid, null, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_User_Returns_Not_Found_If_Invalid_User_Uuid()
        {
            var organization = await CreateOrganizationAsync();
            var invalidUserUuid = new Guid();

            _ = await UsersV2Helper.DeleteUserAndVerifyStatusCode(organization.Uuid, invalidUserUuid, null, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Can_Get_Any_User()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var userRequest = CreateCreateUserRequest();
            var user = await UsersV2Helper.CreateUser(organization.Uuid, userRequest); 

            //Act
            var users = await UsersV2Helper.GetUsers(user.Email);

            //Assert
            var responseUser = Assert.Single(users);
            Assert.Equal(user.Uuid, responseUser.Uuid);
        }

        private void AssertUserEqualsUpdateRequest(UpdateUserRequestDTO request, UserResponseDTO response)
        {
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(request.FirstName, response.FirstName);
            Assert.Equal(request.LastName, response.LastName);
            
            AssertBaseUserRequestMatches(request, response);
        }

        private void AssertBaseUserRequestMatches(BaseUserRequestDTO request, UserResponseDTO response)
        {
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

        private async Task<UserResponseDTO> CreateUserAsync(Guid organizationUuid)
        {
            return await UsersV2Helper.CreateUser(organizationUuid, CreateCreateUserRequest());
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
            return $"{nameof(UserInternalApiV2Test)}{A<string>()}";
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
                Roles = A<IEnumerable<OrganizationRoleChoice>>(),
                SendMail = A<bool>()
            };
        }
    }
}
