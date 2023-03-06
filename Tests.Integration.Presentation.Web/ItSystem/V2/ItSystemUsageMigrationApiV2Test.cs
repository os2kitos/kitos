using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.ItSystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    public class ItSystemUsageMigrationApiV2Test : WithAutoFixture
    {
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Can_Get_Specific_Unused_It_System(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var (itSystem, organization) = await CreatePrerequisites(accessModifier: AccessModifier.Public);

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, itSystem.Name, 10, true, cookie);

            //Assert
            var system = Assert.Single(systems);
            Assert.Equal(itSystem.Uuid, system.Uuid);
        }
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Can_Limit_How_Many_Systems_To_Return(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var prefix = "1";//A<string>();
            var system1Name = prefix + CreateName();
            var system2Name = prefix + CreateName();
            var (itSystem, organization) = await CreatePrerequisites(system1Name);
            var itSystem2 = await CreateSystemAsync(organization.Id, system2Name);
            var createdSystemsUuids = new[] { itSystem.Uuid, itSystem2.Uuid };

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, prefix, 1, true, cookie);

            //Assert
            var system = Assert.Single(systems);
            Assert.Contains(system.Uuid, createdSystemsUuids);
        }

        private async Task<(ItSystemDTO system, OrganizationDTO organization)> CreatePrerequisites(string systemName = null, AccessModifier accessModifier = AccessModifier.Local)
        {
            var organization = await CreateOrganizationAsync();
            var system = await CreateSystemAsync(organization.Id, systemName ?? CreateName(), accessModifier);

            return (system, organization);
        }

        private static async Task<ItSystemDTO> CreateSystemAsync(
            int organizationId,
            string name,
            AccessModifier accessModifier = AccessModifier.Local)
        {
            return await ItSystemHelper.CreateItSystemInOrganizationAsync(name, organizationId, accessModifier);
        }

        protected async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "11224455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return nameof(ItSystemUsageMigrationApiV2Test) + A<string>();
        }
    }
}
