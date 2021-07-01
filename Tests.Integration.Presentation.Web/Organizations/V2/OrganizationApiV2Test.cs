using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.External.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;
using OrganizationType = Presentation.Web.Models.External.V2.Types.OrganizationType;

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

        [Fact]
        public async Task GET_Organization_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var orgType = A<OrganizationTypeKeys>();
            var newOrg = await CreateOrganizationAsync(orgType);

            //Act
            var dto = await OrganizationV2Helper.GetOrganizationAsync(regularUserToken.Token, newOrg.Uuid);

            //Assert
            Assert.Equal(newOrg.Uuid, dto.Uuid);
            Assert.Equal(newOrg.Name, dto.Name);
            AssertOrganizationType(orgType, dto);
            Assert.Equal(newOrg.GetActiveCvr(), dto.Cvr);
        }

        [Fact]
        public async Task GET_Organization_Returns_NotFound()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var orgType = A<OrganizationTypeKeys>();

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
            var orgType = A<OrganizationTypeKeys>();

            //Act
            using var response = await OrganizationV2Helper.SendGetOrganizationAsync(regularUserToken.Token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static readonly IReadOnlyDictionary<OrganizationTypeKeys, OrganizationType> InnerToExternalOrgType =
            new ReadOnlyDictionary<OrganizationTypeKeys, OrganizationType>(new Dictionary<OrganizationTypeKeys, OrganizationType>()
            {
                {OrganizationTypeKeys.Kommune, OrganizationType.Municipality},
                {OrganizationTypeKeys.Interessefællesskab, OrganizationType.CommunityOfInterest},
                {OrganizationTypeKeys.Virksomhed, OrganizationType.Company},
                {OrganizationTypeKeys.AndenOffentligMyndighed, OrganizationType.OtherPublicAuthority}
            });

        private static readonly IReadOnlyDictionary<int, OrganizationType> KnownTypes =
            new ReadOnlyDictionary<int, OrganizationType>(new Dictionary<int, OrganizationType>()
            {
                {1, OrganizationType.Municipality},
                {2, OrganizationType.CommunityOfInterest},
                {3, OrganizationType.Company},
                {4, OrganizationType.OtherPublicAuthority}
            });
        private static void AssertOrganizationType(OrganizationTypeKeys createdWith, OrganizationResponseDTO dto)
        {
            var expectedResult = InnerToExternalOrgType[createdWith];
            Assert.Equal(expectedResult, dto.OrganizationType);
        }

        private async Task<Organization> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(OrganizationApiV2Test)}{A<string>()}";
        }

        //TODO: GET Many, with/without params
        //TODO: GET single

        private string CreateEmail()
        {
            return $"{nameof(OrganizationApiV2Test)}{DateTime.Now.Ticks}{A<Guid>():N}@kitos.dk";
        }
    }
}
