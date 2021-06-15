using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.V2;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    public class ItSystemsApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_GET_Public_ItSystem_As_Stakeholder_If_Placed_In_Other_Organization()
        {
            //Arrange - create user in new org and mark as stakeholder - then ensure that public data can be read from another org
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (entityUuid,_) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            //Act
            var system = await ItSystemV2Helper.GetSingleAsync(token, entityUuid);

            //Assert
            Assert.NotNull(system);
            Assert.Equal(entityUuid, system.Uuid);
        }

        [Fact]
        public async Task Can_GET_Local_ItSystem_As_Stakeholder_If_Placed_In_Organization_Which_User_Is_Member_Of()
        {
            //Arrange - create user in new org and mark as stakeholder - then ensure that public data can be read from another org
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (entityUuid, _) = await CreateSystemAsync(organization.Id, AccessModifier.Local);

            //Act
            var system = await ItSystemV2Helper.GetSingleAsync(token, entityUuid);

            //Assert
            Assert.NotNull(system);
            Assert.Equal(entityUuid, system.Uuid);
        }

        [Fact]
        public async Task Cannot_GET_ItSystem_As_Stakeholder_If_Local_System_Placed_In_Organization_Which_User_Is_Not_Member_Of()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (entityUuid, _) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

            //Act
            using var systemResponse = await ItSystemV2Helper.SendGetSingleAsync(token, entityUuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, systemResponse.StatusCode);
        }

        [Fact]
        public async Task GET_ItSystem_Returns_Expected_Data()
        {
            //Arrange
            //TODO
            //Act

            //Assert - data is set as expected. Interfaces are filtered by access control so that only interfaces, which are accessible to the user, are pointed out
            Assert.True(false);
        }

        [Fact]
        public async Task GET_Many_Without_Filters()
        {
            //Arrange - make sure there are always systems to satisfy the test regardless of order
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            await CreateSystemAsync(organization.Id, AccessModifier.Local);
            await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            //Act
            var systems = await ItSystemV2Helper.GetManyAsync(token, pageSize: 2);

            //Assert
            Assert.Equal(2, systems.Count());
        }

        [Fact]
        public async Task GET_Many_With_RightsHolderFilter()
        {
            //Arrange - make sure there are always systems to satisfy the test regardless of order
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var rightsHolder = await CreateOrganizationAsync();

            var unExpectedAsItIsLocalInNonMemberOrg = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var expected1 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var expected2 = await CreateSystemAsync(organization.Id, AccessModifier.Local);

            using var resp1 = await ItSystemHelper.SendSetBelongsToRequestAsync(unExpectedAsItIsLocalInNonMemberOrg.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp2 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected1.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp3 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected2.dbId, rightsHolder.Id, organization.Id);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyAsync(token, rightsHolderId: rightsHolder.Uuid)).ToList();

            //Assert - only 2 are actually valid since the excluded one was hidden to the stakeholder
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems,dto=>dto.Uuid == expected1.uuid);
            Assert.Contains(systems,dto=>dto.Uuid == expected2.uuid);
        }

        [Fact]
        public async Task GET_Many_With_BusinessTypeFilter()
        {
            //Arrange
            //TODO
            //Act

            //Assert
            Assert.True(false);
        }

        [Fact]
        public async Task GET_Many_With_KLE_Number_Filter()
        {
            //Arrange
            //TODO
            //Act

            //Assert
            Assert.True(false);
        }

        [Fact]
        public async Task GET_Many_With_KLE_Uuid_Filter()
        {
            //Arrange
            //TODO
            //Act

            //Assert
            Assert.True(false);
        }

        [Fact]
        public async Task GET_Many_With_NumberOfUsers_Filter()
        {
            //Arrange
            //TODO
            //Act

            //Assert
            Assert.True(false);
        }

        private async Task<(Guid uuid, int dbId)> CreateSystemAsync(int organizationId, AccessModifier accessModifier)
        {
            var systemName = CreateName();
            var createdSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, accessModifier);
            var entityUuid = TestEnvironment.GetEntityUuid<Core.DomainModel.ItSystem.ItSystem>(createdSystem.Id);

            return (entityUuid, createdSystem.Id);
        }

        private async Task<(string token, Organization createdOrganization)> CreateStakeHolderUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.User, organization.Id, true, true);
            return (token, organization);
        }

        private async Task<Organization> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "11224455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(ItSystemsApiV2Test)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}@kitos.dk";
        }
    }
}
