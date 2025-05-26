using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Shared;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.ItSystemUsage;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    public class ItSystemUsageMigrationApiV2Test : BaseTest
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
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, itSystem.Name, cookie);

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
            var expectedNumberOfSystems = 2;
            var prefix = A<string>();
            var system1Name = prefix + A<string>();
            var system2Name = prefix + A<string>();
            var system3Name = prefix + A<string>();

            var (itSystem, organization, cookie) = await CreatePrerequisitesWithUser(role, system1Name);
            var itSystem2 = await CreateItSystemAsync(organization.Uuid, system2Name);
            var itSystem3 = await CreateItSystemAsync(organization.Uuid, system3Name);
            var createdSystemsUuids = new[] { itSystem.Uuid, itSystem2.Uuid, itSystem3.Uuid };

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, expectedNumberOfSystems, prefix, cookie);

            //Assert
            var systemList = systems.ToList();
            Assert.Equal(expectedNumberOfSystems, systemList.Count);
            Assert.True(systemList.All(x => createdSystemsUuids.Contains(x.Uuid)));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Can_Include_Public_It_Systems_From_Other_Organizations_And_All_From_Own(OrganizationRole role)
        {
            //Arrange
            var prefix = A<string>();
            var system1Name = prefix + A<string>();
            var system2Name = prefix + A<string>();
            var system3Name = prefix + A<string>();

            var (ownLocalSystem, organization, cookie) = await CreatePrerequisitesWithUser(role, system1Name);
            var itSystem2 = await CreateItSystemAsync(DefaultOrgUuid, system2Name);
            await CreateItSystemAsync(DefaultOrgUuid, system3Name, RegistrationScopeChoice.Local);
            var createdSystemsUuids = new[] { ownLocalSystem.Uuid, itSystem2.Uuid };

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 3, prefix, cookie);

            //Assert
            var systemList = systems.ToList();
            Assert.Equal(2, systemList.Count);
            Assert.True(systemList.All(x => createdSystemsUuids.Contains(x.Uuid)));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Filters_By_Name(OrganizationRole role)
        {
            //Arrange
            var system1Name = A<string>();
            var system2Name = A<string>();

            var (itSystem, organization, cookie) = await CreatePrerequisitesWithUser(role, system1Name, AccessModifier.Public);
            await CreateItSystemAsync(organization.Uuid, system2Name);

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, system1Name, cookie);

            //Assert
            var dto = Assert.Single(systems);
            Assert.Equal(dto.Uuid, itSystem.Uuid);
            Assert.Equal(dto.Name, itSystem.Name);
        }

        [Fact, Description("Systems in use in our own organization should not be included")]
        public async Task GetUnusedItSystems_Does_Not_Include_Systems_In_Use()
        {
            //Arrange
            var prefix = A<Guid>().ToString("N");
            var system1Name = $"{prefix}_1";
            var system2Name = $"{prefix}_2";

            var (itSystem, organization, cookie) = await CreatePrerequisitesWithUser(OrganizationRole.GlobalAdmin, system1Name, AccessModifier.Public);
            var localSystem = await CreateItSystemAsync(organization.Uuid, system2Name);
            await TakeSystemIntoUsageAsync(localSystem.Uuid, organization.Uuid);

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, prefix, cookie);

            //Assert
            var systemList = systems.ToList();
            var dto = Assert.Single(systemList);
            Assert.Equal(itSystem.Uuid, dto.Uuid);
        }

        [Fact, Description("Systems in use applies to current org only, do not expect results to be omitted if it system is used in another organization.")]
        public async Task GetUnusedItSystems_Does_Not_Include_Filter_Systems_In_Use_In_Other_Orgs()
        {
            //Arrange
            var prefix = A<string>();
            var system1Name = prefix + A<string>();
            var system2Name = prefix + A<string>();

            var (itSystem, organization, cookie) = await CreatePrerequisitesWithUser(OrganizationRole.GlobalAdmin, system1Name, AccessModifier.Public);
            var itSystemOtherOrg = await CreateItSystemAsync(DefaultOrgUuid, system2Name);
            await TakeSystemIntoUsageAsync(itSystemOtherOrg.Uuid, organization.Uuid);

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 2, prefix, cookie);

            //Assert
            var systemList = systems.ToList();
            var dto = Assert.Single(systemList);
            Assert.Equal(itSystem.Uuid, dto.Uuid);
        }

        [Theory]
        [InlineData(OrganizationRole.User, false)]
        [InlineData(OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        public async Task GetAccessibilityLevel_Returns(OrganizationRole role, bool expectedMigrationAvailability)
        {
            //Arrange
            var (_, _, cookie) = await CreatePrerequisitesWithUser(role);

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetPermissions(cookie);

            //Assert
            var command = Assert.Single(result.Commands, x => x.Id == CommandPermissionCommandIds.UsageMigration.Execute);
            Assert.Equal(expectedMigrationAvailability, command.CanExecute);
        }

        [Fact]
        public async Task GetMigration_When_System_Migration_Has_No_Horizontal_Consequences()
        {
            //Arrange
            var (system, organization) = await CreatePrerequisites();
            var newSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var usage = await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetMigration(usage.Uuid, newSystem.Uuid);

            //Assert
            Assert.Empty(result.AffectedRelations);
            Assert.Empty(result.AffectedContracts);
            Assert.Empty(result.AffectedDataProcessingRegistrations);
            AssertFromToSystemInfo(usage, result, system, newSystem);
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Associated_In_Contract()
        {
            //Arrange
            var (system, organization) = await CreatePrerequisites();
            var newSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var usage = await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);

            var contract = await CreateItContractAsync(organization.Uuid);
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract.Uuid,
                usage.Uuid.WrapAsEnumerable());

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetMigration(usage.Uuid, newSystem.Uuid);

            //Assert
            Assert.Empty(result.AffectedRelations);
            Assert.Empty(result.AffectedDataProcessingRegistrations);

            var affectedContract = Assert.Single(result.AffectedContracts);
            Assert.Equal(contract.Uuid, affectedContract.Uuid);

            AssertFromToSystemInfo(usage, result, system, newSystem);
        }

        [Fact]
        public async Task GetMigration_When_System_Is_Associated_In_DataProcessingRegistration()
        {
            //Arrange
            var (system, organization) = await CreatePrerequisites();
            var newSystem = await CreateItSystemAsync(organization.Uuid);
            var usage = await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);

            var dpr = await CreateDPRAsync(organization.Uuid);
            await DataProcessingRegistrationV2Helper.PatchSystemsAsync(dpr.Uuid, usage.Uuid.WrapAsEnumerable());

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetMigration(usage.Uuid, newSystem.Uuid);

            //Assert
            Assert.Empty(result.AffectedRelations);
            Assert.Empty(result.AffectedContracts);

            var affectedDpr = Assert.Single(result.AffectedDataProcessingRegistrations);
            Assert.Equal(dpr.Uuid, affectedDpr.Uuid);

            AssertFromToSystemInfo(usage, result, system, newSystem);
        }

        [Fact]
        public async Task GetMigration_When_System_Has_Relation_With_From_System_Exposing_Interface()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);

            var interfaceDto = await CreateItInterfaceAsync(organization.Uuid);
            var exhibit =
                await InterfaceV2Helper.PatchExposedBySystemAsync(interfaceDto.Uuid,
                    fromSystem.Uuid);

            var relationDTO = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), toSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = fromSystemUsage.Uuid,
                    RelationInterfaceUuid = interfaceDto.Uuid
                });

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetMigration(fromSystemUsage.Uuid, toSystem.Uuid);

            //Assert
            Assert.Empty(result.AffectedDataProcessingRegistrations);
            Assert.Empty(result.AffectedContracts);

            var affectedRelation = Assert.Single(result.AffectedRelations);
            Assert.Equal(relationDTO.ToSystemUsage.Name, affectedRelation.ToSystem.Name);
            Assert.Equal(toSystem.Name, affectedRelation.FromSystem.Name); //Why??
            Assert.Equal(relationDTO.Description, affectedRelation.Description);
            Assert.Equal(relationDTO.RelationInterface.Name, affectedRelation.Interface.Name);
            Assert.Null(affectedRelation.Contract);
            Assert.Null(affectedRelation.FrequencyType);

            AssertFromToSystemInfo(fromSystemUsage, result, fromSystem, toSystem);
        }

        [Fact]
        public async Task GetMigration_When_System_Has_Relation_With_No_RelationInterface()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), fromSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = toSystemUsage.Uuid
                });

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetMigration(fromSystemUsage.Uuid, toSystem.Uuid);

            //Assert
            Assert.Empty(result.AffectedDataProcessingRegistrations);
            Assert.Empty(result.AffectedContracts);
            Assert.Empty(result.AffectedRelations);

            AssertFromToSystemInfo(fromSystemUsage, result, fromSystem, toSystem);
        }

        [Fact]
        public async Task GetMigration_When_System_Has_Relation_With_Contract_And_No_RelationInterface()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);

            await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), fromSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = toSystemUsage.Uuid,
                    AssociatedContractUuid = contract.Uuid
                });

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetMigration(fromSystemUsage.Uuid, toSystem.Uuid);

            //Assert
            Assert.Empty(result.AffectedDataProcessingRegistrations);
            Assert.Empty(result.AffectedContracts);
            Assert.Empty(result.AffectedRelations);

            AssertFromToSystemInfo(fromSystemUsage, result, fromSystem, toSystem);
        }

        [Fact]
        public async Task PostMigration_Can_Migrate_System_Usage_With_No_Associated_Entities()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());

            //Act and Assert (expected no content)
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, toSystem.Uuid);
        }

        [Fact]
        public async Task PostMigration_SystemUsageAssociation_In_Contract_Is_Not_Changed()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var contract = await CreateItContractAsync(organization.Uuid);
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract.Uuid,
                fromSystemUsage.Uuid.WrapAsEnumerable());

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, toSystem.Uuid);

            //Assert
            await AssertSystemUsageAssociationExistsInContract(contract, fromSystemUsage);
        }

        [Fact]
        public async Task PostMigration_Can_Migrate_All_Usage_Data()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);

            var contract = await CreateItContractAsync(organization.Uuid);
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract.Uuid,
                fromSystemUsage.Uuid.WrapAsEnumerable());

            var interfaceDto = await CreateItInterfaceAsync(organization.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(interfaceDto.Uuid, toSystem.Uuid);

            var frequencyType =
                await OptionV2ApiHelper.GetRandomOptionAsync(
                    OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid);

            var relation = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), fromSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = toSystemUsage.Uuid,
                    RelationInterfaceUuid = interfaceDto.Uuid,
                    AssociatedContractUuid = contract.Uuid,
                    Description = A<string>(),
                    RelationFrequencyUuid = frequencyType.Uuid
                });

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, toSystem.Uuid);

            //Assert
            await AssertSystemUsageAssociationExistsInContract(contract, fromSystemUsage);
            await AssertRelationExists(relation, fromSystemUsage, true, true, true);
        }

        [Fact]
        public async Task PostMigration_To_Own_System_Does_Nothing()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);

            var contract = await CreateItContractAsync(organization.Uuid);
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract.Uuid,
                fromSystemUsage.Uuid.WrapAsEnumerable());

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, fromSystem.Uuid);

            //Assert
            await AssertSystemUsageAssociationExistsInContract(contract, fromSystemUsage);
        }

        [Fact]
        public async Task PostMigration_When_System_Has_Relation_With_From_System_Exposing_Interface()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);

            var interfaceDto = await CreateItInterfaceAsync(organization.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(interfaceDto.Uuid, toSystem.Uuid);

            var relation = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(),
                fromSystemUsage.Uuid, new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = toSystemUsage.Uuid,
                    RelationInterfaceUuid = interfaceDto.Uuid
                });

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, toSystem.Uuid);

            //Assert
            await AssertRelationExists(relation, fromSystemUsage, true);
        }

        [Fact]
        public async Task PostMigration_When_System_Has_Relation_With_To_System_Exposing_Interface()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid);
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);

            var interfaceDto = await CreateItInterfaceAsync(organization.Uuid);
            await InterfaceV2Helper.SendPatchExposedBySystemAsync(await GetGlobalToken(), interfaceDto.Uuid, toSystem.Uuid);

            var migrateToItSystem = await CreateItSystemAsync(organization.Uuid);

            var relation = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), fromSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = toSystemUsage.Uuid,
                    RelationInterfaceUuid = interfaceDto.Uuid
                });

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(toSystemUsage.Uuid, migrateToItSystem.Uuid);

            //Assert
            await AssertRelationExists(relation, fromSystemUsage);
        }

        [Fact]
        public async Task PostMigration_When_System_Has_Relation_With_To_System()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);

            var migrateToItSystem = await CreateItSystemAsync(organization.Uuid);

            var relation = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), fromSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = toSystemUsage.Uuid,

                });

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, migrateToItSystem.Uuid);

            //Assert
            await AssertRelationExists(relation, fromSystemUsage);
        }

        [Fact]
        public async Task PostMigration_When_System_Has_Relation_With_Contract()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await TakeSystemIntoUsageAsync(fromSystem.Uuid, organization.Uuid);
            var toSystem = await CreateItSystemAsync(organization.Uuid, CreateName());
            var toSystemUsage = await TakeSystemIntoUsageAsync(toSystem.Uuid, organization.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);

            var migrateToItSystem = await CreateItSystemAsync(organization.Uuid);

            var relation = await ItSystemUsageV2Helper.PostRelationAsync(await GetGlobalToken(), fromSystemUsage.Uuid,
                new SystemRelationWriteRequestDTO
                {
                    ToSystemUsageUuid = toSystemUsage.Uuid,
                    AssociatedContractUuid = contract.Uuid
                });

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, migrateToItSystem.Uuid);

            //Assert
            await AssertRelationExists(relation, fromSystemUsage, hasContract: true);
        }

        private static void AssertFromToSystemInfo(
            ItSystemUsageResponseDTO usage,
            ItSystemUsageMigrationV2ResponseDTO result,
            ItSystemResponseDTO oldSystem,
            ItSystemResponseDTO newSystem)
        {
            Assert.Equal(usage.Uuid, result.TargetUsage.Uuid);
            Assert.Equal(oldSystem.Uuid, result.FromSystem.Uuid);
            Assert.Equal(newSystem.Uuid, result.ToSystem.Uuid);
        }

        private static async Task AssertSystemUsageAssociationExistsInContract(ItContractResponseDTO contract, ItSystemUsageResponseDTO usage)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var contractFromServer = await ItContractV2Helper.GetItContractAsync(token.Token, contract.Uuid);

            Assert.Equal(1, contractFromServer.SystemUsages.Count(x => x.Uuid == usage.Uuid));
        }

        private static async Task AssertRelationExists(OutgoingSystemRelationResponseDTO expectedRelation, ItSystemUsageResponseDTO usage, bool hasInterface = false, bool hasFrequency = false, bool hasContract = false)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var relation = await ItSystemUsageV2Helper.GetRelationAsync(token.Token, usage.Uuid, expectedRelation.Uuid);

            Assert.Equal(expectedRelation.Uuid, relation.Uuid);
            Assert.Equal(expectedRelation.Description, relation.Description);
            if (hasInterface)
            {
                AssertEqualNamedEntities(expectedRelation.RelationInterface, relation.RelationInterface);
            }
            else
            {
                Assert.Null(relation.RelationInterface);
            }

            if (hasFrequency)
            {
                AssertEqualNamedEntities(expectedRelation.RelationFrequency, relation.RelationFrequency);
            }
            else
            {
                Assert.Null(relation.RelationFrequency);
            }

            if (hasContract)
            {
                AssertEqualNamedEntities(expectedRelation.AssociatedContract, relation.AssociatedContract);
            }
            else
            {
                Assert.Null(relation.AssociatedContract);
            }
        }

        private static void AssertEqualNamedEntities(IdentityNamePairResponseDTO expected, IdentityNamePairResponseDTO actual)
        {
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Name, actual.Name);
        }

        private async Task<(ItSystemResponseDTO system, ShallowOrganizationResponseDTO organization, Cookie cookie)> CreatePrerequisitesWithUser(OrganizationRole role, string systemName = null, AccessModifier accessModifier = AccessModifier.Local)
        {
            var (system, organization) = await CreatePrerequisites(systemName, accessModifier);
            var user = await HttpApi.CreateUserAndLogin(CreateEmail(), role, organization.Uuid);

            return (system, organization, user.loginCookie);
        }

        private async Task<(ItSystemResponseDTO system, ShallowOrganizationResponseDTO organization)> CreatePrerequisites(string systemName = null, AccessModifier accessModifier = AccessModifier.Local)
        {
            var organization = await CreateOrganizationAsync();
            var system = await CreateItSystemAsync(organization.Uuid, systemName, accessModifier.ToChoice());

            return (system, organization);
        }

        private string CreateName()
        {
            return nameof(ItSystemUsageMigrationApiV2Test) + A<string>();
        }
    }
}
