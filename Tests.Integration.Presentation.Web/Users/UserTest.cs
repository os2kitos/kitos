﻿using Core.DomainModel;
using Core.DomainModel.Organization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Users
{
    public class UserTest : WithAutoFixture
    {

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
    }
}
