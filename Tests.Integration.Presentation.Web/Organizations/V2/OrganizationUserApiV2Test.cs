using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationUserApiV2Test : BaseTest
    {
        [Fact]
        public async Task Can_GET_Organization_Users()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization);
            var user2 = await CreateUser(organization);
            var user3AndToken = await CreateApiUser(organization);

            var rolesForUser3 = new[] { OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, OrganizationRole.SystemModuleAdmin };
            await AssignRoles(organization, user3AndToken.user.Uuid, rolesForUser3);

            //Act
            var result = (await OrganizationUserV2Helper.GetOrganizationUsersAsync(user1AndToken.token, organization.Uuid)).ToList();

            //Assert
            Assert.Equal(3, result.Count);
            ExpectResult(result, user1AndToken.user, OrganizationUserRole.User);
            ExpectResult(result, user2, OrganizationUserRole.User);
            ExpectResult(result, user3AndToken.user, OrganizationUserRole.User, OrganizationUserRole.LocalAdmin, OrganizationUserRole.OrganizationModuleAdmin, OrganizationUserRole.SystemModuleAdmin);
        }

        [Fact]
        public async Task Can_GET_Organization_Users_Filter_By_Name()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization);
            var user2 = await CreateUser(organization);

            //Act
            var result = (await OrganizationUserV2Helper.GetOrganizationUsersAsync(user1AndToken.token, organization.Uuid, nameOrEmailQuery: $"{user2.Name} {user2.LastName}")).ToList();

            //Assert
            var dto = Assert.Single(result);
            AssertUser(user2, dto, OrganizationUserRole.User);
        }

        [Fact]
        public async Task Can_GET_Organization_Users_Filter_By_Email()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization);
            var user2 = await CreateUser(organization);
            var user3 = await CreateUser(organization);

            var rolesForUser3 = new[] { OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, OrganizationRole.SystemModuleAdmin };
            await AssignRoles(organization, user3.Uuid, rolesForUser3);

            //Act
            var result = (await OrganizationUserV2Helper.GetOrganizationUsersAsync(user1AndToken.token, organization.Uuid, nameOrEmailQuery: $"{user2.Email.Split('@')[0]}")).ToList();

            //Assert
            var dto = Assert.Single(result);
            AssertUser(user2, dto, OrganizationUserRole.User);
        }

        [Fact]
        public async Task Can_GET_Organization_Users_Filter_By_Email_Only()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization);
            var user2 = await CreateUser(organization);
            var user3 = await CreateUser(organization);

            var rolesForUser3 = new[] { OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, OrganizationRole.SystemModuleAdmin };
            await AssignRoles(organization, user3.Uuid, rolesForUser3);

            //Act
            var result = (await OrganizationUserV2Helper.GetOrganizationUsersAsync(user1AndToken.token, organization.Uuid, emailQuery: user2.Email)).ToList();

            //Assert
            var dto = Assert.Single(result);
            AssertUser(user2, dto, OrganizationUserRole.User);
        }

        [Fact]
        public async Task Can_GET_Organization_Users_Filter_By_Role()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization);
            var user3 = await CreateUser(organization);

            var rolesForUser3 = new[] { OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, OrganizationRole.SystemModuleAdmin };
            await AssignRoles(organization, user3.Uuid, rolesForUser3);

            //Act
            var result = (await OrganizationUserV2Helper.GetOrganizationUsersAsync(user1AndToken.token, organization.Uuid, roleQuery: OrganizationUserRole.SystemModuleAdmin)).ToList();

            //Assert
            var dto = Assert.Single(result);
            AssertUser(user3, dto, OrganizationUserRole.User, OrganizationUserRole.LocalAdmin, OrganizationUserRole.OrganizationModuleAdmin, OrganizationUserRole.SystemModuleAdmin);
        }

        [Fact]
        public async Task Cannot_GET_Organization_Users_In_Organization_I_Dont_Belong_To()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync();
            var organization2 = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization1);

            //Act
            using var result = await OrganizationUserV2Helper.SendGetOrganizationUsersAsync(user1AndToken.token, organization2.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Organization_Users_In_Organization_That_Does_Not_Exist()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization1);

            //Act
            using var result = await OrganizationUserV2Helper.SendGetOrganizationUsersAsync(user1AndToken.token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Can_GET_Organization_User()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization);
            var user2 = await CreateUser(organization);

            //Act
            var result = await OrganizationUserV2Helper.GetOrganizationUserAsync(user1AndToken.token, organization.Uuid, user2.Uuid);

            //Assert
            AssertUser(user2, result, OrganizationUserRole.User);
        }

        [Fact]
        public async Task Cannot_GET_Organization_User_If_Requested_User_Has_No_Roles_In_Target_Organization()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync();
            var organization2 = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization1);
            var user2 = await CreateUser(organization2);

            //Act
            using var result = await OrganizationUserV2Helper.SendGetOrganizationUserAsync(user1AndToken.token, organization1.Uuid, user2.Uuid); //user 2 is only in org1

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Organization_User_If_I_Dont_Belong_To_Organization()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync();
            var organization2 = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization1);
            var user2 = await CreateUser(organization2);

            //Act
            using var result = await OrganizationUserV2Helper.SendGetOrganizationUserAsync(user1AndToken.token, organization2.Uuid, user2.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Organization_User_If_User_Does_Not_Exist()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization1);
            var unknownUserId = A<Guid>();

            //Act
            using var result = await OrganizationUserV2Helper.SendGetOrganizationUserAsync(user1AndToken.token, organization1.Uuid, unknownUserId);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Organization_User_If_Organization_Does_Not_Exist()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync();
            var user1AndToken = await CreateApiUser(organization1);

            //Act
            using var result = await OrganizationUserV2Helper.SendGetOrganizationUserAsync(user1AndToken.token, A<Guid>(), user1AndToken.user.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        private static void ExpectResult(List<OrganizationUserResponseDTO> result, User expectedUserSource, params OrganizationUserRole[] roles)
        {
            var dto = Assert.Single(result.Where(x => x.Uuid == expectedUserSource.Uuid));
            AssertUser(expectedUserSource, dto, roles);
        }

        private static void AssertUser(User dbUser, OrganizationUserResponseDTO dtoUser, params OrganizationUserRole[] roles)
        {
            var expectedRoles = new HashSet<OrganizationUserRole>(roles);

            Assert.Equal(dbUser.Uuid, dtoUser.Uuid);
            Assert.Equal(dbUser.GetFullName(), dtoUser.Name);
            Assert.Equal(dbUser.Name, dtoUser.FirstName);
            Assert.Equal(dbUser.LastName, dtoUser.LastName);
            Assert.Equal(dbUser.PhoneNumber, dtoUser.PhoneNumber);
            Assert.Equal(dbUser.Email, dtoUser.Email);
            Assert.Equal(dbUser.HasApiAccess.GetValueOrDefault(false), dtoUser.ApiAccess);
            Assert.Equal(expectedRoles, dtoUser.Roles.ToHashSet());
        }

        private async Task<(User user, string token)> CreateApiUser(ShallowOrganizationResponseDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Uuid, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid));
            return (user, userAndGetToken.token);
        }

        private async Task<User> CreateUser(ShallowOrganizationResponseDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, organization.Uuid, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid));
            return user;
        }

        private static async Task AssignRoles(ShallowOrganizationResponseDTO organization, Guid userUuid, params OrganizationRole[] roles)
        {
            foreach (var organizationRole in roles)
            {
                using var response = await HttpApi.SendAssignRoleToUserAsync(userUuid, organizationRole, organization.Uuid);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
