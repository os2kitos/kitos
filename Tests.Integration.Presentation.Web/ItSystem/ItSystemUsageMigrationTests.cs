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
        private ItSystemDTO _newSystem;

        public async Task InitializeAsync()
        {
            _oldSystemInUse = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            _oldSystemUsage = await ItSystemHelper.TakeIntoUseAsync(_oldSystemInUse.Id, _oldSystemInUse.OrganizationId);
            _newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
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
            var usage = _oldSystemUsage;
            var newSystem = _newSystem;

            //Act
            using (var response = await GetMigration(usage, newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedContracts);
                Assert.Empty(result.AffectedItProjects);
                AssertFromToSystemInfo(usage, result, _oldSystemInUse, newSystem);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Used_In_A_Project()
        {
            //Arrange
            var project = await ItProjectHelper.CreateProject(CreateName(), TestEnvironment.DefaultOrganizationId);
            await ItProjectHelper.AddSystemBinding(project.Id, _oldSystemUsage.Id, TestEnvironment.DefaultOrganizationId);

            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedContracts);
                Assert.Equal(1, result.AffectedItProjects?.Count());
                Assert.Equal(project.Id, result.AffectedItProjects?.FirstOrDefault()?.Id);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Associated_In_Contract()
        {
            //Arrange
            var contract = await ItContractHelper.CreateContract(CreateName(), TestEnvironment.DefaultOrganizationId);
            await ItContractHelper.AddItSystemUsage(contract.Id, _oldSystemUsage.Id, TestEnvironment.DefaultOrganizationId);

            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedItProjects);
                Assert.Equal(1, result.AffectedContracts?.Count());

                var dto = result.AffectedContracts?.FirstOrDefault();
                Assert.Equal(contract.Id, dto?.Contract?.Id);
                Assert.Empty(dto.AffectedInterfaceUsages);
                Assert.Empty(dto.InterfaceExhibitUsagesToBeDeleted);
                Assert.True(dto.SystemAssociatedInContract);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Associated_Through_UseInterface_Mappings_In_Contract()
        {
            //Arrange
            var createdInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var contract = await ItContractHelper.CreateContract(CreateName(), TestEnvironment.DefaultOrganizationId);
            var usage = await InterfaceUsageHelper.CreateAsync(contract.Id, _oldSystemUsage.Id, _oldSystemInUse.Id, createdInterface.Id, TestEnvironment.DefaultOrganizationId);

            //Adding an unaffected usage (not same system usage source)
            var unaffectedItSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var unaffectedUsage = await ItSystemHelper.TakeIntoUseAsync(unaffectedItSystem.Id, TestEnvironment.DefaultOrganizationId);
            await InterfaceUsageHelper.CreateAsync(contract.Id, unaffectedUsage.Id, unaffectedItSystem.Id, createdInterface.Id, TestEnvironment.DefaultOrganizationId);

            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedItProjects);
                Assert.Equal(1, result.AffectedContracts?.Count());
                var migrationDto = result.AffectedContracts?.First();
                Assert.Empty(migrationDto.InterfaceExhibitUsagesToBeDeleted);
                Assert.Equal(1, migrationDto.AffectedInterfaceUsages?.Count());
                AssertInterfaceMapping(usage, migrationDto.AffectedInterfaceUsages?.First());
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Associated_Through_InterfaceExhibit_Mappings_In_Contract()
        {
            //Arrange
            var createdInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var contract = await ItContractHelper.CreateContract(CreateName(), TestEnvironment.DefaultOrganizationId);
            var exhibit = await InterfaceExhibitHelper.CreateExhibit(_oldSystemInUse.Id, createdInterface.Id);
            var exhibitUsage = await InterfaceExhibitHelper.CreateExhibitUsage(contract.Id, _oldSystemUsage.Id, exhibit.Id);

            //Adding an unaffected exhibit usage (not same system usage source)
            var unaffectedItSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var unaffectedInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var unaffectedUsage = await ItSystemHelper.TakeIntoUseAsync(unaffectedItSystem.Id, TestEnvironment.DefaultOrganizationId);
            var unAffectedExhibit = await InterfaceExhibitHelper.CreateExhibit(unaffectedItSystem.Id, unaffectedInterface.Id);
            await InterfaceExhibitHelper.CreateExhibitUsage(contract.Id, unaffectedUsage.Id, unAffectedExhibit.Id);


            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedItProjects);
                Assert.Equal(1, result.AffectedContracts?.Count());
                var migrationDto = result.AffectedContracts?.First();
                Assert.Empty(migrationDto.AffectedInterfaceUsages);
                Assert.Equal(1, migrationDto.InterfaceExhibitUsagesToBeDeleted?.Count());
                AssertInterfaceMapping(exhibitUsage, migrationDto.InterfaceExhibitUsagesToBeDeleted?.First());
            }
        }

        private static async Task<HttpResponseMessage> GetMigration(ItSystemUsageDTO usage, ItSystemDTO toSystem)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration?usageId={usage.Id}&toSystemId={toSystem.Id}");

            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        private static void AssertInterfaceMapping(ItInterfaceUsageDTO createdBinding, NamedEntityDTO affectedUsage)
        {
            Assert.Equal(createdBinding.ItInterfaceId, affectedUsage.Id);
            Assert.Equal(createdBinding.ItInterfaceItInterfaceName, affectedUsage.Name);
        }

        private static void AssertInterfaceMapping(ItInterfaceExhibitUsageDTO createdBinding, NamedEntityDTO affectedUsage)
        {
            Assert.Equal(createdBinding.ItInterfaceExhibitItInterfaceId, affectedUsage.Id);
            Assert.Equal(createdBinding.ItInterfaceExhibitItInterfaceName, affectedUsage.Name);
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
