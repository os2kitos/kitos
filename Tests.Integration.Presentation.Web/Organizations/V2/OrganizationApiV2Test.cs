using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Xunit;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationApiV2Test : OrganizationApiV2TestBase
    {
        [Fact]
        public async Task GET_Organizations_With_RightsHolders_Access_Returns_Empty_If_User_Does_Not_Have_RightsHoldersAccessInAnyOrganization()
        {
            //Arrange
            var email = CreateEmail();
            var userDetails = await HttpApi.CreateUserAndGetToken(email, OrganizationRole.User, DefaultOrgUuid, true);

            //Act
            var organizations = await OrganizationV2Helper.GetOrganizationsForWhichUserIsRightsHolder(userDetails.token);

            //Assert
            Assert.Empty(organizations);
        }

        [Fact]
        public async Task GET_Organizations_With_RightsHolders_Access_Returns_Organizations_Where_User_Has_RightsHolderAccessRole()
        {
            //Arrange
            var email = CreateEmail();
            var userDetails = await HttpApi.CreateUserAndGetToken(email, OrganizationRole.User, DefaultOrgUuid, true);
            using var response1 = await HttpApi.SendAssignRoleToUserAsync(userDetails.userUuid, OrganizationRole.RightsHolderAccess, SecondOrgUuid);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            var secondOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.SecondOrganizationId);
            var firstOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            var organizations = (await OrganizationV2Helper.GetOrganizationsForWhichUserIsRightsHolder(userDetails.token)).ToList();

            //Assert
            var organization = Assert.Single(organizations);
            Assert.Equal(secondOrgUuid, organization.Uuid);
            Assert.NotNull(organization.Name);

            //Assign another org and observe the change
            using var response2 = await HttpApi.SendAssignRoleToUserAsync(userDetails.userUuid, OrganizationRole.RightsHolderAccess, DefaultOrgUuid);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            organizations = (await OrganizationV2Helper.GetOrganizationsForWhichUserIsRightsHolder(userDetails.token)).ToList();
            Assert.Equal(2, organizations.Count);
            Assert.Equal(new[] { firstOrgUuid, secondOrgUuid }.OrderBy(x => x), organizations.Select(x => x.Uuid).OrderBy(x => x));
        }

        [Fact]
        public async Task GET_Organization_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var orgType = A<OrganizationType>();
            var newOrg = await CreateOrganizationAsync(orgType);

            //Act
            var dto = await OrganizationV2Helper.GetOrganizationAsync(regularUserToken.Token, newOrg.Uuid);

            //Assert
            Assert.Equal(newOrg.Uuid, dto.Uuid);
            Assert.Equal(newOrg.Name, dto.Name);
            AssertOrganizationType(orgType, dto);
            Assert.Equal(newOrg.Cvr, dto.Cvr);
        }

        [Fact]
        public async Task GET_Organization_Returns_NotFound()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await OrganizationV2Helper.SendGetOrganizationAsync(regularUserToken.Token, Guid.NewGuid());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GET_Organization_Returns_BadRequest_For_Empty_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await OrganizationV2Helper.SendGetOrganizationAsync(regularUserToken.Token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Organizations_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250);

            //Assert
            Assert.NotEmpty(organizations);
        }

        [Fact]
        public async Task GET_Organizations_Returns_Ok_With_Name_Content_Filtering()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newOrg = await CreateOrganizationAsync(A<OrganizationType>());

            //Act
            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250, nameContent: newOrg.Name);

            //Assert
            var org = Assert.Single(organizations);
            Assert.Equal(newOrg.Uuid, org.Uuid);
        }

        [Fact]
        public async Task GET_Organizations_Returns_Ok_OnlyMyOrganizations_Filtering()
        {
            //Arrange
            var newOrg = await CreateOrganizationAsync(OrganizationType.Municipality);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, newOrg.Uuid, true, false);

            //Act
            var organizationsWithFiltering = await OrganizationV2Helper.GetOrganizationsAsync(token, 0, 2, onlyWhereUserHasMembership: true);
            var organizationsWithOutFiltering = await OrganizationV2Helper.GetOrganizationsAsync(token, 0, 2, onlyWhereUserHasMembership: false);

            //Assert
            Assert.Equal(2, organizationsWithOutFiltering.Count());
            var org = Assert.Single(organizationsWithFiltering);
            Assert.Equal(newOrg.Uuid, org.Uuid);
        }

        [Fact]
        public async Task GET_Organizations_Returns_Ok_Cvr_Filtering()
        { //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newOrg = await CreateOrganizationAsync(A<OrganizationType>());

            //Act
            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250, cvrContent: newOrg.Cvr);

            //Assert
            Assert.All(organizations, organization => Assert.Equal(newOrg.Cvr, organization.Cvr));
        }

        [Fact]
        public async Task GET_Organizations_Returns_Ok_Uuid_Filtering()
        { //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newOrg = await CreateOrganizationAsync(A<OrganizationType>());

            //Act
            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250, uuid: newOrg.Uuid);

            //Assert
            var org = Assert.Single(organizations);
            Assert.Equal(newOrg.Uuid, org.Uuid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GET_Organizations_Returns_Ok_NameOrCvr_Filtering(bool inputIsCvr)
        { //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newOrg = await CreateOrganizationAsync(A<OrganizationType>());

            //Act
            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250, nameOrCvrContent: inputIsCvr ? newOrg.Cvr : newOrg.Name);

            //Assert
            var org = Assert.Single(organizations.Where(x => x.Uuid == newOrg.Uuid));
            Assert.Equal(newOrg.Uuid, org.Uuid);
        }

        private static void AssertOrganizationType(OrganizationType createdWith, OrganizationResponseDTO dto)
        {
            Assert.Equal(createdWith, dto.OrganizationType);
        }
    }
}
