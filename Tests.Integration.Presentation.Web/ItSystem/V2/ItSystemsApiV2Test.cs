using System;
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
            var entityUuid = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

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
            var entityUuid = await CreateSystemAsync(organization.Id, AccessModifier.Local);

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
            var entityUuid = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

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
            //Arrange
            //TODO
            //Act

            //Assert
            Assert.True(false);
        }

        [Fact]
        public async Task GET_Many_With_Pagination()
        {
            //Arrange
            //TODO
            //Act

            //Assert
            Assert.True(false);
        }

        [Fact]
        public async Task GET_Many_With_RightsHolderFilter()
        {
            //Arrange
            //TODO
            //Act

            //Assert
            Assert.True(false);
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

        private async Task<Guid> CreateSystemAsync(int organizationId, AccessModifier accessModifier)
        {
            var systemName = CreateName();
            var publicSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName,
                organizationId, accessModifier);
            var entityUuid = TestEnvironment.GetEntityUuid<Core.DomainModel.ItSystem.ItSystem>(publicSystem.Id);
            return entityUuid;
        }

        private async Task<(string token, Organization createdOrganization)> CreateStakeHolderUserInNewOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "11224455", OrganizationTypeKeys.Virksomhed,
                AccessModifier.Public);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.User, organization.Id, true, true);
            return (token, organization);
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
