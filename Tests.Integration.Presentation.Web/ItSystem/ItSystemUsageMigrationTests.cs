using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Presentation.Web.Models.ItSystemUsageMigration;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageMigrationTests : IAsyncLifetime
    {
        private ItSystemDTO _oldSystemInUse;
        private ItSystemUsageDTO _oldSystemUsage;

        public async Task InitializeAsync()
        {
            _oldSystemInUse = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            _oldSystemUsage = await ItSystemHelper.TakeIntoUseAsync(_oldSystemInUse.Id, _oldSystemInUse.OrganizationId);
        }

        public Task DisposeAsync()
        {
            //Nothing to clean up
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_Specific_Unused_It_System(OrganizationRole role)
        {
            //Arrange
            var itSystemName = CreateName();
            var itSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(itSystemName, TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration/UnusedItSystems" +
                                                $"?organizationId={TestEnvironment.DefaultOrganizationId}" +
                                                $"&nameContent={itSystemName}" +
                                                $"&numberOfItSystems={10}" +
                                                $"&getPublicFromOtherOrganizations={true}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemSimpleDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystems = response.ToList();
                Assert.Single(itSystems);
                Assert.Equal(itSystemName, itSystems[0].Name);
                Assert.Equal(itSystem.Id, itSystems[0].Id);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Limit_How_Many_Systems_To_Return(OrganizationRole role)
        {
            //Arrange
            var prefix = CreateName();
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var itSystemName3 = prefix + CreateName();
            var itSystem1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(itSystemName1, TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var itSystem2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(itSystemName2, TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var itSystem3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(itSystemName3, TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var createdSystemsIds = new[] { itSystem1.Id, itSystem2.Id, itSystem3.Id };

            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration/UnusedItSystems" +
                                                $"?organizationId={TestEnvironment.DefaultOrganizationId}" +
                                                $"&nameContent={prefix}" +
                                                $"&numberOfItSystems={2}" +
                                                $"&getPublicFromOtherOrganizations={true}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemSimpleDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystems = response.ToList();
                Assert.Equal(2, itSystems.Count);
                Assert.True(itSystems.All(x => createdSystemsIds.Contains(x.Id)));
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_Public_It_Systems_From_Other_Organizations_And_All_From_Own(OrganizationRole role)
        {
            //Arrange
            var prefix = CreateName();
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var itSystemName3 = prefix + CreateName();
            var ownLocalSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(itSystemName1, TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var sharedSystemFromOtherOrg = await ItSystemHelper.CreateItSystemInOrganizationAsync(itSystemName2, TestEnvironment.SecondOrganizationId, AccessModifier.Public);
            await ItSystemHelper.CreateItSystemInOrganizationAsync(itSystemName3, TestEnvironment.SecondOrganizationId, AccessModifier.Local); //Private system in other org

            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration/UnusedItSystems" +
                                                $"?organizationId={TestEnvironment.DefaultOrganizationId}" +
                                                $"&nameContent={prefix}" +
                                                $"&numberOfItSystems={3}" +
                                                $"&getPublicFromOtherOrganizations={true}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemSimpleDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystemIds = response.Select(x => x.Id).ToList();
                Assert.Equal(2, itSystemIds.Count);
                Assert.Contains(ownLocalSystem.Id, itSystemIds);
                Assert.Contains(sharedSystemFromOtherOrg.Id, itSystemIds);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.User, false)]
        [InlineData(OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        public async Task GetAccessibilityLevel_Returns(OrganizationRole role, bool expectedMigrationAvailability)
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/v1/ItSystemUsageMigration/Accessibility");
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = await response.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageMigrationAccessDTO>();
                Assert.Equal(expectedMigrationAvailability, result.CanExecuteMigration);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Not_Used_In_Contracts_Or_Projects()
        {
            //Arrange
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration?usageId={_oldSystemUsage.Id}&toSystemId={newSystem.Id}");

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedContracts);
                Assert.Empty(result.AffectedItProjects);
                AssertFromToSystemInfo(_oldSystemUsage, result, _oldSystemInUse, newSystem);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Used_In_A_Project()
        {
            //Arrange
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var project = await ItProjectHelper.CreateProject(CreateName(), TestEnvironment.DefaultOrganizationId);
            await ItProjectHelper.AddSystemBinding(project.Id, _oldSystemUsage.Id, TestEnvironment.DefaultOrganizationId);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration?usageId={_oldSystemUsage.Id}&toSystemId={newSystem.Id}");

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedContracts);
                AssertFromToSystemInfo(_oldSystemUsage, result, _oldSystemInUse, newSystem);
                Assert.Equal(1, result.AffectedItProjects?.Count());
                Assert.Equal(project.Id, result.AffectedItProjects?.FirstOrDefault()?.Id);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_UseInterface_Mappings_In_Contract()
        {
            //Arrange
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var createdInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var contract = await ItContractHelper.CreateContract(CreateName(), TestEnvironment.DefaultOrganizationId);
            var usage = await InterfaceUsageHelper.CreateAsync(contract.Id, _oldSystemUsage.Id, _oldSystemInUse.Id, createdInterface.Id, TestEnvironment.DefaultOrganizationId);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration?usageId={_oldSystemUsage.Id}&toSystemId={newSystem.Id}");

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedItProjects);
                AssertFromToSystemInfo(_oldSystemUsage, result, _oldSystemInUse, newSystem);
                Assert.Equal(1, result.AffectedContracts?.Count());
                var migrationDto = result.AffectedContracts?.First();
                Assert.Empty(migrationDto.InterfaceExhibitUsagesToBeDeleted);
                Assert.Equal(1, migrationDto.AffectedInterfaceUsages?.Count());
                AssertInterfaceMapping(usage, migrationDto.AffectedInterfaceUsages?.First());
            }
        }

        private static void AssertInterfaceMapping(ItInterfaceUsageDTO createdBinding, NamedEntityDTO affectedUsage)
        {
            Assert.Equal(createdBinding.Id, affectedUsage.Id);
            Assert.Equal(createdBinding.Interface.Name, affectedUsage.Name);
        }

        private static async Task<ItSystemUsageMigrationDTO> AssertMigrationReturned(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageMigrationDTO>();
            Assert.NotNull(result);
            return result;
        }

        private static void AssertFromToSystemInfo(
            ItSystemUsageDTO usage,
            ItSystemUsageMigrationDTO result,
            ItSystemDTO oldSystem,
            ItSystemDTO newSystem)
        {
            Assert.Equal(usage.Id, result.TargetUsage.Id);
            Assert.Equal(oldSystem.Id, result.FromSystem.Id);
            Assert.Equal(newSystem.Id, result.ToSystem.Id);
        }

        private static string CreateName()
        {
            return $"{Guid.NewGuid():N}";
        }
    }
}
