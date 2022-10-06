using System;
using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainServices.Extensions;
using Infrastructure.DataAccess.Extensions;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External.Rights;
using Tests.Integration.Presentation.Web.Tools.Internal.UI_Configuration;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Users
{
    public class UserTest : WithAutoFixture
    {
        private static readonly EntityPropertyProxyValueLoader<User> ProxyLoader = new();

        [Fact]
        public async Task Can_Get_Users_And_Organizations_Where_User_Has_RightsholderAccess()
        {
            //Arrange
            var (userId, userEmail, organization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();

            //Act
            var result = await UserHelper.GetUsersWithRightsholderAccessAsync();

            //Assert
            var userOrgDTO = Assert.Single(result.Where(x => x.Id == userId));
            Assert.Equal(userEmail, userOrgDTO.Email);
            Assert.Equal(organization.Name, userOrgDTO.OrgName);
            Assert.True(userOrgDTO.ApiAccess);
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Get_Users_And_Organizations_Where_User_Has_RightsholderAccess_If_Not_GlobalAdmin(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using var result = await UserHelper.SendGetUsersWithRightsholderAccessAsync(cookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Users_Where_User_Has_StakeHolder_Or_ApiAccess()
        {
            //Arrange
            var (userId1, userEmail1, orgName1) = await CreateStakeHolderUserInNewOrganizationAsync(true, true);
            var (userId2, userEmail2, orgName2) = await CreateStakeHolderUserInNewOrganizationAsync(false, true);
            var (userId3, userEmail3, orgName3) = await CreateStakeHolderUserInNewOrganizationAsync(true, false);
            var (userId4, userEmail4, orgName4) = await CreateStakeHolderUserInNewOrganizationAsync(false, false);

            //Act
            var result = await UserHelper.GetUsersWithCrossAccessAsync();

            //Assert
            var userOrgDTO1 = Assert.Single(result.Where(x => x.Id == userId1));
            Assert.Equal(userEmail1, userOrgDTO1.Email);
            Assert.True(userOrgDTO1.StakeholderAccess);
            Assert.True(userOrgDTO1.ApiAccess);
            var orgWhereActive1 = Assert.Single(userOrgDTO1.OrganizationsWhereActive);
            Assert.Equal(orgName1, orgWhereActive1);

            var userOrgDTO2 = Assert.Single(result.Where(x => x.Id == userId2));
            Assert.Equal(userEmail2, userOrgDTO2.Email);
            Assert.True(userOrgDTO2.StakeholderAccess);
            Assert.False(userOrgDTO2.ApiAccess);
            var orgWhereActive2 = Assert.Single(userOrgDTO2.OrganizationsWhereActive);
            Assert.Equal(orgName2, orgWhereActive2);

            var userOrgDTO3 = Assert.Single(result.Where(x => x.Id == userId3));
            Assert.Equal(userEmail3, userOrgDTO3.Email);
            Assert.False(userOrgDTO3.StakeholderAccess);
            Assert.True(userOrgDTO3.ApiAccess);
            var orgWhereActive3 = Assert.Single(userOrgDTO3.OrganizationsWhereActive);
            Assert.Equal(orgName3, orgWhereActive3);

            Assert.Empty(result.Where(x => x.Id == userId4));
        }

        [Fact]
        public async Task Can_Get_Users_Where_User_Has_StakeHolder_Or_ApiAccess_Returns_Distinct_Organisations_Only()
        {
            //Arrange
            var email = CreateEmail();
            var organization = await CreateOrganizationAsync();
            var userId = await HttpApi.CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(email, true, true), OrganizationRole.User, organization.Id);
            using var assignRoleResult = await HttpApi.SendAssignRoleToUserAsync(userId, OrganizationRole.LocalAdmin, organization.Id); // Add extra role in organisation to have 2 organisation roles. 
            Assert.Equal(HttpStatusCode.Created, assignRoleResult.StatusCode);

            //Act
            var result = await UserHelper.GetUsersWithCrossAccessAsync();

            //Assert
            var userOrgDTO = Assert.Single(result.Where(x => x.Id == userId));
            Assert.Equal(email, userOrgDTO.Email);
            Assert.True(userOrgDTO.StakeholderAccess);
            Assert.True(userOrgDTO.ApiAccess);
            var orgWhereActive = Assert.Single(userOrgDTO.OrganizationsWhereActive);
            Assert.Equal(organization.Name, orgWhereActive);

        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Get_Users_Where_User_Has_StakeHolder_Or_ApiAccess_If_Not_GlobalAdmin(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using var result = await UserHelper.SendGetUsersWithCrossAccessAsync(cookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Delete_User()
        {
            var userRole = OrganizationRole.LocalAdmin;

            var (_, userId, organization, originalEmail) = await CreatePrerequisitesAsync(userRole);
            var name = A<string>();

            await RightsHelper.AddUserRole(userId, organization.Id, RightsType.ItContractRights, name);
            await RightsHelper.AddUserRole(userId, organization.Id, RightsType.OrganizationUnitRights, name);
            await RightsHelper.AddUserRole(userId, organization.Id, RightsType.ItSystemRights, name);
            await RightsHelper.AddDprRoleToUser(userId, organization.Id, name);

            SsoIdentityHelper.AddSsoIdentityToUser(userId);
            
            var deleteResponse = await UserHelper.SendDeleteUserAsync(userId);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            DatabaseAccess.MapFromEntitySet<User, User>(repository =>
            {
                var user = repository.AsQueryable().ById(userId);
                if (user == null)
                {
                    throw new ArgumentException("Failed to find user with id", nameof(userId));
                }

                user.Transform(ProxyLoader.LoadReferencedEntities);

                Assert.True(user.Deleted);
                Assert.False(user.IsGlobalAdmin);
                Assert.False(user.HasApiAccess);
                Assert.False(user.HasStakeHolderAccess);

                Assert.Contains("_deleted_user@kitos.dk", user.Email);
                Assert.Empty(user.LastName);
                Assert.Equal("Slettet bruger", user.Name);
                Assert.NotNull(user.LockedOutDate);
                Assert.NotNull(user.DeletedDate);
                Assert.Null(user.PhoneNumber);

                Assert.Empty(user.DataProcessingRegistrationRights);
                Assert.Empty(user.OrganizationRights);
                Assert.Empty(user.ItContractRights);
                Assert.Empty(user.ItSystemRights);
                Assert.Empty(user.OrganizationUnitRights);
                Assert.Empty(user.SsoIdentities);

                return user;
            });
        }

        [Fact]
        public async Task Delete_User_Returns_Forbidden_When_User_Tries_To_Delete_Himself()
        {
            var userRole = OrganizationRole.GlobalAdmin;

            var (cookie, userId, _, _) = await CreatePrerequisitesAsync(userRole);

            var deleteResponse = await UserHelper.SendDeleteUserAsync(userId, cookie);
            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Search_User_Returns_Users_With_Matching_Name_Or_Email()
        {
            var userRole = OrganizationRole.User;
            var testPhrase = A<string>();

            var email1 = CreateEmailContainingPhrase(testPhrase);
            var email2 = CreateEmail();
            var email3 = CreateEmail();
            var email4 = CreateEmail();
            var name1 = CreateName();
            var name3 = CreateName();
            var name4 = CreateName();

            var (_, userId1, _, _) = await CreatePrerequisitesAsync(userRole, email1, name1);
            var (_, userId2, _, _) = await CreatePrerequisitesAsync(userRole, email2, testPhrase);
            var (_, userId3, _, _) = await CreatePrerequisitesAsync(userRole, email3, name3);
            var (_, userId4, _, _) = await CreatePrerequisitesAsync(userRole, email4, name4, testPhrase);

            var users = await UserHelper.SearchUsersAsync(testPhrase);

            //Users is expected to contain userId1 because User1 email contains the testPhrase
            Assert.Contains(userId1, users.Select(x => x.Id));

            //Users is expected to contain userId2 User2 name contains the testPhrase
            Assert.Contains(userId2, users.Select(x => x.Id));

            //Users is expected to not contain userId3 because User3 email and name don't contain the testPhrase
            Assert.DoesNotContain(userId3, users.Select(x => x.Id));

            //Users is expected to contain userId4 because User4 last name contains the testPhrase
            Assert.Contains(userId4, users.Select(x => x.Id));
        }

        [Fact]
        public async Task GetUserOrganizations_Returns_User_Organizations()
        {
            var userRole = OrganizationRole.User;
            var (_, userId, org, _) = await CreatePrerequisitesAsync(userRole);

            var response = await OrganizationHelper.SendGetUserOrganizationsRequestAsync(userId);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var organizations = await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<OrganizationSimpleDTO>>();
            Assert.NotNull(organizations);

            var organizationSimpleDtos = organizations.ToList();
            Assert.Contains(org.Id, organizationSimpleDtos.Select(x => x.Id));
            Assert.Single(organizationSimpleDtos);
        }

        private async Task<(Cookie loginCookie, int userId, OrganizationDTO organization, string email)> CreatePrerequisitesAsync(OrganizationRole role, string email = "", string name = "", string lastName = "", bool hasApiAccess = false)
        {
            var organization = await CreateOrganizationAsync();
            var userEmail = string.IsNullOrEmpty(email) ? UIConfigurationHelper.CreateEmail() : email;
            var (userId, _, loginCookie) =
                await HttpApi.CreateUserAndLogin(userEmail, role, name, lastName, organization.Id, hasApiAccess);
            return (loginCookie, userId, organization, userEmail);
        }

        private async Task<(int userId, string userEmail, string orgName)> CreateStakeHolderUserInNewOrganizationAsync(bool hasApiAccess, bool hasStakeholderAccess)
        {
            var email = CreateEmail();
            var organization = await CreateOrganizationAsync();
            var userId = await HttpApi.CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(email, hasApiAccess, hasStakeholderAccess), OrganizationRole.User, organization.Id);
            return (userId, email, organization.Name);
        }

        private async Task<(int userId, string userEmail, OrganizationDTO createdOrganization)> CreateRightsHolderAccessUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();
            var email = CreateEmail();
            var (userId, _, token) = await HttpApi.CreateUserAndGetToken(email,
                OrganizationRole.RightsHolderAccess, organization.Id, true);
            return (userId, email, organization);
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
            return $"{nameof(UserTest)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}@kitos.dk";
        }

        private string CreateEmailContainingPhrase(string phrase)
        {
            return $"{CreateName()}{phrase}@kitos.dk";
        }
    }
}
