using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            _oldSystemInUse = await CreateSystemAsync();
            _oldSystemUsage = await TakeSystemIntoUseAsync(_oldSystemInUse);
            _newSystem = await CreateSystemAsync();
        }

        public Task DisposeAsync()
        {
            //Nothing to clean up
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Can_Get_Specific_Unused_It_System(OrganizationRole role)
        {
            //Arrange
            var itSystemName = CreateName();
            var itSystem = await CreateSystemAsync(name: itSystemName, accessModifier: AccessModifier.Public);

            //Act
            using (var httpResponse = await GetUnusedSystemsAsync(role, TestEnvironment.DefaultOrganizationId, itSystemName, 10, true))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemSimpleDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystems = response.ToList();
                var dto = Assert.Single(itSystems);
                Assert.Equal(itSystemName, dto.Name);
                Assert.Equal(itSystem.Id, dto.Id);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Can_Limit_How_Many_Systems_To_Return(OrganizationRole role)
        {
            //Arrange
            var prefix = CreateName();
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var itSystemName3 = prefix + CreateName();
            var itSystem1 = await CreateSystemAsync(name: itSystemName1);
            var itSystem2 = await CreateSystemAsync(name: itSystemName2);
            var itSystem3 = await CreateSystemAsync(name: itSystemName3);
            var createdSystemsIds = new[] { itSystem1.Id, itSystem2.Id, itSystem3.Id };

            //Act
            using (var httpResponse = await GetUnusedSystemsAsync(role, TestEnvironment.DefaultOrganizationId, prefix, 2, true))
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
        public async Task GetUnusedItSystems_Can_Include_Public_It_Systems_From_Other_Organizations_And_All_From_Own(OrganizationRole role)
        {
            //Arrange
            var prefix = CreateName();
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var itSystemName3 = prefix + CreateName();
            var ownLocalSystem = await CreateSystemAsync(name: itSystemName1);
            var sharedSystemFromOtherOrg = await CreateSystemAsync(TestEnvironment.SecondOrganizationId, itSystemName2, AccessModifier.Public);
            await CreateSystemAsync(TestEnvironment.SecondOrganizationId, itSystemName3); //Private system in other org

            //Act
            using (var httpResponse = await GetUnusedSystemsAsync(role, TestEnvironment.DefaultOrganizationId, prefix, 3, true))
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
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task GetUnusedItSystems_Can_Include_It_Systems_From_Own_Organization_Only(OrganizationRole role)
        {
            //Arrange
            var prefix = CreateName();
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var ownLocalSystem = await CreateSystemAsync(name: itSystemName1);
            await CreateSystemAsync(TestEnvironment.SecondOrganizationId, itSystemName2, AccessModifier.Public); //shared system should not be returned when not asked to

            //Act
            using (var httpResponse = await GetUnusedSystemsAsync(role, TestEnvironment.DefaultOrganizationId, prefix, 2, false))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemSimpleDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystemIds = response.Select(x => x.Id).ToList();
                var id = Assert.Single(itSystemIds);
                Assert.Equal(ownLocalSystem.Id, id);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, " ")]
        [InlineData(OrganizationRole.GlobalAdmin, "")]
        public async Task GetUnusedItSystems_Returns_Empty_List_On_Whitespace_Or_Empty(OrganizationRole role, string nameContent)
        {
            //Act
            using (var httpResponse = await GetUnusedSystemsAsync(role, TestEnvironment.DefaultOrganizationId, nameContent, 25, true))
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(TestEnvironment.EmptyListApiJson, response);
            }
        }

        [Fact, Description("Systems in use in our own organization should not be included")]
        public async Task GetUnusedItSystems_Does_Not_Include_Systems_In_Use()
        {
            //Arrange
            var prefix = CreateName();
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var itSystemName3 = prefix + CreateName();
            var itSystem1 = await CreateSystemAsync(name: itSystemName1);

            //These two - one local and one shared in other org - should not be returned since they are in use in own organization
            var itSystem2 = await CreateSystemAsync(name: itSystemName2);
            var itSystem3 = await CreateSystemAsync(name: itSystemName3, organizationId: TestEnvironment.SecondOrganizationId, accessModifier: AccessModifier.Public);
            await TakeSystemIntoUseAsync(itSystem2, organizationId: TestEnvironment.DefaultOrganizationId);
            await TakeSystemIntoUseAsync(itSystem3, organizationId: TestEnvironment.DefaultOrganizationId);

            //Act
            using (var httpResponse = await GetUnusedSystemsAsync(OrganizationRole.GlobalAdmin, TestEnvironment.DefaultOrganizationId, prefix, 3, true))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemSimpleDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystems = response.ToList();
                var dto = Assert.Single(itSystems);
                Assert.Equal(itSystem1.Id, dto.Id);
            }
        }

        [Fact, Description("Systems in use applies to current org only, do not expect results to be omitted if it system is used in another organization.")]
        public async Task GetUnusedItSystems_Does_Not_Include_Filter_Systems_In_Use_In_Other_Orgs()
        {
            //Arrange
            var prefix = CreateName();
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var itSystemName3 = prefix + CreateName();
            var itSystem1 = await CreateSystemAsync(name: itSystemName1);

            //These two - one local and one shared in other org - should not be returned since they are in use in own organization
            var itSystem2 = await CreateSystemAsync(name: itSystemName2);
            var itSystem3 = await CreateSystemAsync(name: itSystemName3, organizationId: TestEnvironment.SecondOrganizationId, accessModifier: AccessModifier.Public);
            await TakeSystemIntoUseAsync(itSystem2, organizationId: TestEnvironment.SecondOrganizationId);
            await TakeSystemIntoUseAsync(itSystem3, organizationId: TestEnvironment.SecondOrganizationId);
            var createdSystemsIds = new[] { itSystem1.Id, itSystem2.Id, itSystem3.Id };

            //Act
            using (var httpResponse = await GetUnusedSystemsAsync(OrganizationRole.GlobalAdmin, TestEnvironment.DefaultOrganizationId, prefix, 3, true))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemSimpleDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystems = response.ToList();
                Assert.Equal(3, itSystems.Count);
                Assert.True(itSystems.All(x => createdSystemsIds.Contains(x.Id)));
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
            var project = await CreateProjectAsync();
            await AddProjectSystemBindingAsync(project, _oldSystemUsage);

            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedContracts);
                var dto = Assert.Single(result.AffectedItProjects);
                Assert.Equal(project.Id, dto.Id);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Associated_In_Contract()
        {
            //Arrange
            var contract = await CreateContractAsync();
            await AddItSystemUsageToContractAsync(contract, _oldSystemUsage);

            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedItProjects);
                var dto = Assert.Single(result.AffectedContracts);
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
            var createdInterface = await CreateInterfaceAsync();
            var contract = await CreateContractAsync();
            var usage = await CreateInterfaceUsageAsync(contract, createdInterface, _oldSystemUsage, _oldSystemInUse);

            //Adding an unaffected usage (not same system usage source)
            var unaffectedItSystem = await CreateSystemAsync();
            var unaffectedUsage = await TakeSystemIntoUseAsync(unaffectedItSystem);
            await CreateInterfaceUsageAsync(contract, createdInterface, unaffectedUsage, unaffectedItSystem);

            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedItProjects);
                var migrationDto = Assert.Single(result.AffectedContracts);
                Assert.Empty(migrationDto.InterfaceExhibitUsagesToBeDeleted);
                var dto = Assert.Single(migrationDto.AffectedInterfaceUsages);
                AssertInterfaceMapping(usage, dto);
            }
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Associated_Through_InterfaceExhibit_Mappings_In_Contract()
        {
            //Arrange
            var createdInterface = await CreateInterfaceAsync();
            var contract = await CreateContractAsync();
            var exhibit = await CreateExhibitAsync(createdInterface, _oldSystemInUse);
            var exhibitUsage = await CreateExhibitUsageAsync(contract, exhibit, _oldSystemUsage);

            //Adding an unaffected exhibit usage (not same system usage source)
            var unaffectedItSystem = await CreateSystemAsync();
            var unaffectedInterface = await CreateInterfaceAsync();
            var unaffectedUsage = await TakeSystemIntoUseAsync(unaffectedItSystem);
            var unAffectedExhibit = await CreateExhibitAsync(unaffectedInterface, unaffectedItSystem);
            await CreateExhibitUsageAsync(contract, unAffectedExhibit, unaffectedUsage);

            //Act
            using (var response = await GetMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                var result = await AssertMigrationReturned(response);
                Assert.Empty(result.AffectedItProjects);
                var migrationDto = Assert.Single(result.AffectedContracts);
                Assert.Empty(migrationDto.AffectedInterfaceUsages);
                var dto = Assert.Single(migrationDto.InterfaceExhibitUsagesToBeDeleted);
                AssertInterfaceMapping(exhibitUsage, dto);
            }
        }

        [Fact]
        public async Task PostMigration_Can_Migrate_System_Usage_With_No_Associated_Entities()
        {
            //Act
            using (var response = await PostMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                AssertMigrationSucceeded(response);
            }
        }

        [Fact]
        public async Task PostMigration_Results_In_Interface_Exhibit_Usage_Being_Removed()
        {
            //Arrange
            var createdInterface = await CreateInterfaceAsync();
            var contract = await CreateContractAsync();
            var exhibit = await CreateExhibitAsync(createdInterface, _oldSystemInUse);
            var exhibitUsage = await CreateExhibitUsageAsync(contract, exhibit, _oldSystemUsage);

            //Adding a usage which should not be affected
            var unaffectedItSystem = await CreateSystemAsync();
            var unaffectedInterface = await CreateInterfaceAsync();
            var unaffectedUsage = await TakeSystemIntoUseAsync(unaffectedItSystem);
            var unAffectedExhibit = await CreateExhibitAsync(unaffectedInterface, unaffectedItSystem);
            var unaffectedExhibitUsage = await CreateExhibitUsageAsync(contract, unAffectedExhibit, unaffectedUsage);

            //Act
            using (var response = await PostMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                AssertMigrationSucceeded(response);
                await AssertExhibitExists(contract, exhibitUsage, false);
                await AssertExhibitExists(contract, unaffectedExhibitUsage, true);
            }
        }

        [Fact]
        public async Task PostMigration_Results_In_Interface_Usage_Being_Updated()
        {
            //Arrange
            var createdInterface = await CreateInterfaceAsync();
            var contract = await CreateContractAsync();
            var interfaceUsage = await CreateInterfaceUsageAsync(contract, createdInterface, _oldSystemUsage, _oldSystemInUse);

            //Adding an unaffected usage (not same system usage source)
            var unaffectedItSystem = await CreateSystemAsync();
            var unaffectedUsage = await TakeSystemIntoUseAsync(unaffectedItSystem);
            var unaffectedInterfaceUsage = await CreateInterfaceUsageAsync(contract, createdInterface, unaffectedUsage, unaffectedItSystem);

            //Act
            using (var response = await PostMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                AssertMigrationSucceeded(response);
                await AssertInterfaceUsageUpdatedAfterMigration(interfaceUsage, _newSystem.Id);
                await AssertInterfaceUsageSystemBinding(unaffectedInterfaceUsage, unaffectedItSystem.Id);
            }
        }

        [Fact]
        public async Task PostMigration_SystemUsageAssociation_In_Contract_Is_Not_Changed()
        {
            //Arrange
            var contract = await CreateContractAsync();
            await AddItSystemUsageToContractAsync(contract, _oldSystemUsage);
            var createdContract = await GetItContractAsync(contract.Id);

            //Act
            using (var response = await PostMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                AssertMigrationSucceeded(response);
                await AssertSystemUsageAssociationExistsInContract(createdContract, _oldSystemUsage);
            }
        }

        [Fact]
        public async Task PostMigration_Associated_Project_Is_Not_Changed()
        {
            //Arrange
            var project = await CreateProjectAsync();
            await AddProjectSystemBindingAsync(project, _oldSystemUsage);
            var updatedFromItSystemUsage = await GetItSystemUsageAsync(_oldSystemUsage.Id);
            //Act
            using (var response = await PostMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                AssertMigrationSucceeded(response);
                await AssertAssociatedProjectExists(updatedFromItSystemUsage, project);
            }
        }

        [Fact]
        public async Task PostMigration_Access_Types_Are_Removed()
        {
            //Arrange
            var accessType1 = CreateName();
            var accessType2 = CreateName();
            var type1 = await ItSystemHelper.CreateAccessTypeAsync(_oldSystemInUse.Id, accessType1);
            var type2 = await ItSystemHelper.CreateAccessTypeAsync(_oldSystemInUse.Id, accessType2);

            await ItSystemHelper.EnableAccessTypeAsync(_oldSystemUsage.Id, type1.Id);
            await ItSystemHelper.EnableAccessTypeAsync(_oldSystemUsage.Id, type2.Id);
            var enabledAccessTypes = await ItSystemHelper.GetEnabledAccessTypesAsync(_oldSystemUsage.Id);

            //Make sure the access types were added
            Assert.True(
                new[]
                    {
                        type1.Id,
                        type2.Id
                    }
                    .OrderBy(x => x).SequenceEqual(
                        enabledAccessTypes
                            .Select(x => x.Id)
                            .OrderBy(x => x))
            );

            //Act
            using (var response = await PostMigration(_oldSystemUsage, _newSystem))
            {
                //Assert - access types should have been removed
                AssertMigrationSucceeded(response);
                enabledAccessTypes = await ItSystemHelper.GetEnabledAccessTypesAsync(_oldSystemUsage.Id);
                Assert.Empty(enabledAccessTypes);
            }
        }

        [Fact]
        public async Task PostMigration_Can_Migrate_All_Usage_Data()
        {
            //Arrange
            var project = await CreateProjectAsync();
            await AddProjectSystemBindingAsync(project, _oldSystemUsage);
            var contract = await CreateContractAsync();
            await AddItSystemUsageToContractAsync(contract, _oldSystemUsage);
            var createdInterface = await CreateInterfaceAsync();
            var exhibit = await CreateExhibitAsync(createdInterface, _oldSystemInUse);
            var exhibitUsage = await CreateExhibitUsageAsync(contract, exhibit, _oldSystemUsage);
            var interfaceUsage = await CreateInterfaceUsageAsync(contract, createdInterface, _oldSystemUsage, _oldSystemInUse);

            var createdContract = await GetItContractAsync(contract.Id);

            //Act
            using (var response = await PostMigration(_oldSystemUsage, _newSystem))
            {
                //Assert
                AssertMigrationSucceeded(response);
                await AssertExhibitExists(contract, exhibitUsage, false);
                await AssertInterfaceUsageUpdatedAfterMigration(interfaceUsage, _newSystem.Id);
                await AssertSystemUsageAssociationExistsInContract(createdContract, _oldSystemUsage);
                await AssertAssociatedProjectExists(_oldSystemUsage, project);
            }
        }

        [Fact]
        public async Task PostMigration_To_Own_System_Does_Nothing()
        {
            //Arrange
            var project = await CreateProjectAsync();
            await AddProjectSystemBindingAsync(project, _oldSystemUsage);
            var contract = await CreateContractAsync();
            await AddItSystemUsageToContractAsync(contract, _oldSystemUsage);
            var createdInterface = await CreateInterfaceAsync();
            var exhibit = await CreateExhibitAsync(createdInterface, _oldSystemInUse);
            var exhibitUsage = await CreateExhibitUsageAsync(contract, exhibit, _oldSystemUsage);
            var interfaceUsage = await CreateInterfaceUsageAsync(contract, createdInterface, _oldSystemUsage, _oldSystemInUse);

            var createdContract = await GetItContractAsync(contract.Id);

            //Act
            using (var response = await PostMigration(_oldSystemUsage, _oldSystemInUse))
            {
                //Assert
                AssertMigrationSucceeded(response);
                await AssertExhibitExists(contract, exhibitUsage, true);
                await AssertInterfaceUsageSystemBinding(interfaceUsage, _oldSystemInUse.Id);
                await AssertSystemUsageAssociationExistsInContract(createdContract, _oldSystemUsage);
                await AssertAssociatedProjectExists(_oldSystemUsage, project);
            }
        }

        private static async Task<HttpResponseMessage> PostMigration(ItSystemUsageDTO usage, ItSystemDTO toSystem)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration?usageId={usage.Id}&toSystemId={toSystem.Id}");

            return await HttpApi.PostWithCookieAsync(url, cookie, null);
        }

        private static async Task<HttpResponseMessage> GetUnusedSystemsAsync(OrganizationRole role, int organizationId, string nameContent, int take, bool getPublic)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"api/v1/ItSystemUsageMigration/UnusedItSystems" +
                                                $"?organizationId={organizationId}" +
                                                $"&nameContent={nameContent}" +
                                                $"&numberOfItSystems={take}" +
                                                $"&getPublicFromOtherOrganizations={getPublic}");

            return await HttpApi.GetWithCookieAsync(url, cookie);
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

        private static void AssertMigrationSucceeded(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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

        private static async Task AssertExhibitExists(ItContractDTO contract, ItInterfaceExhibitUsageDTO exhibit, bool exists)
        {
            var exhibitUsages = await InterfaceExhibitHelper.GetExhibitInterfaceUsages(contract.Id);
            Assert.Equal(exists ? 1 : 0, exhibitUsages.Count(x => x.ItSystemUsageId == exhibit.ItSystemUsageId && x.ItInterfaceExhibitItInterfaceId == exhibit.ItInterfaceExhibitItInterfaceId));
        }

        private static async Task AssertInterfaceUsageUpdatedAfterMigration(ItInterfaceUsageDTO interfaceUsage, int sysId)
        {
            // Checking the old interface usage key is removed
            using (var response = await InterfaceUsageHelper.GetItInterfaceUsageResponse(
                interfaceUsage.ItSystemUsageId,
                interfaceUsage.ItSystemId,
                interfaceUsage.ItInterfaceId))
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


            await AssertInterfaceUsageSystemBinding(interfaceUsage, sysId);
        }

        private static async Task AssertInterfaceUsageSystemBinding(ItInterfaceUsageDTO interfaceUsage, int sysId)
        {
            var newInterfaceUsage = await InterfaceUsageHelper.GetItInterfaceUsage(
                interfaceUsage.ItSystemUsageId,
                sysId,
                interfaceUsage.ItInterfaceId);

            Assert.Equal(interfaceUsage.ItSystemUsageId, newInterfaceUsage.ItSystemUsageId);
            Assert.Equal(sysId, newInterfaceUsage.ItSystemId);
            Assert.Equal(interfaceUsage.ItInterfaceId, newInterfaceUsage.ItInterfaceId);
        }

        private static async Task AssertSystemUsageAssociationExistsInContract(ItContractDTO contract, ItSystemUsageDTO usage)
        {
            var contractFromServer = await GetItContractAsync(contract.Id);

            Assert.Equal(1, contractFromServer.AssociatedSystemUsages.Count(x => x.Id == usage.Id));
        }

        private static async Task AssertAssociatedProjectExists(ItSystemUsageDTO oldItSystemUsage, ItProjectDTO project)
        {
            var itSystemUsage = await GetItSystemUsageAsync(oldItSystemUsage.Id);

            Assert.Equal(1, itSystemUsage.ItProjects.Count(x => x.Id == project.Id));
        }

        private static Task<ItSystemDTO> CreateSystemAsync(
            int organizationId = TestEnvironment.DefaultOrganizationId,
            string name = null,
            AccessModifier accessModifier = AccessModifier.Local)
        {
            return ItSystemHelper.CreateItSystemInOrganizationAsync(name ?? CreateName(), organizationId, accessModifier);
        }

        private static async Task<ItInterfaceExhibitDTO> CreateExhibitAsync(ItInterfaceDTO exposedInterface, ItSystemDTO exposingSystem)
        {
            return await InterfaceExhibitHelper.CreateExhibit(exposingSystem.Id, exposedInterface.Id);
        }

        private static async Task<ItSystemUsageDTO> TakeSystemIntoUseAsync(ItSystemDTO system, int? organizationId = null)
        {
            return await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId ?? system.OrganizationId);
        }

        private static async Task<ItContractDTO> CreateContractAsync()
        {
            return await ItContractHelper.CreateContract(CreateName(), TestEnvironment.DefaultOrganizationId);
        }

        private static async Task<ItInterfaceDTO> CreateInterfaceAsync()
        {
            var createdInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(),
                CreateName(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            return createdInterface;
        }

        private static async Task<ItInterfaceUsageDTO> CreateInterfaceUsageAsync(ItContractDTO contract, ItInterfaceDTO targetInterface, ItSystemUsageDTO usage, ItSystemDTO system)
        {
            return await InterfaceUsageHelper.CreateAsync(contract.Id, usage.Id, system.Id, targetInterface.Id, TestEnvironment.DefaultOrganizationId);
        }

        private static async Task<ItInterfaceExhibitUsageDTO> CreateExhibitUsageAsync(ItContractDTO contract, ItInterfaceExhibitDTO exhibit, ItSystemUsageDTO systemUsage)
        {
            return await InterfaceExhibitHelper.CreateExhibitUsage(contract.Id, systemUsage.Id, exhibit.Id);
        }

        private static async Task<ItProjectDTO> CreateProjectAsync(int organizationId = TestEnvironment.DefaultOrganizationId)
        {
            return await ItProjectHelper.CreateProject(CreateName(), organizationId);
        }

        private static async Task<ItSystemUsageDTO> AddProjectSystemBindingAsync(ItProjectDTO project, ItSystemUsageDTO systemUsage, int organizationId = TestEnvironment.DefaultOrganizationId)
        {
            return await ItProjectHelper.AddSystemBinding(project.Id, systemUsage.Id, organizationId);
        }

        private static async Task<ItSystemUsageSimpleDTO> AddItSystemUsageToContractAsync(ItContractDTO contract, ItSystemUsageDTO systemUsage, int organizationId = TestEnvironment.DefaultOrganizationId)
        {
            return await ItContractHelper.AddItSystemUsage(contract.Id, systemUsage.Id, organizationId);
        }

        private static async Task<ItContractDTO> GetItContractAsync(int contractId)
        {
            return await ItContractHelper.GetItContract(contractId);
        }

        private static async Task<ItSystemUsageDTO> GetItSystemUsageAsync(int systemUsageId)
        {
            return await ItSystemHelper.GetItSystemUsage(systemUsageId);
        }

        private static string CreateName()
        {
            return $"{Guid.NewGuid():N}";
        }
    }
}
