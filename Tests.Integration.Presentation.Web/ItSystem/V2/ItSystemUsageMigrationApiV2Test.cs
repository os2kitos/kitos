using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.ApplicationServices.Shared;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using ExpectedObjects.Strategies;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.ItSystemUsageMigration;
using Presentation.Web.Models.API.V1.SystemRelations;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
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
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, true, itSystem.Name,  cookie);

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
            var itSystem2 = await CreateSystemAsync(organization.Id, system2Name);
            var itSystem3 = await CreateSystemAsync(organization.Id, system3Name);
            var createdSystemsUuids = new[] { itSystem.Uuid, itSystem2.Uuid, itSystem3.Uuid };

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, expectedNumberOfSystems, true, prefix, cookie);

            //Assert
            var systemList = systems.ToList();
            Assert.Equal(expectedNumberOfSystems, systemList.Count());
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
            var itSystem2 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, system2Name, AccessModifier.Public);
            await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, system3Name);
            var createdSystemsUuids = new[] { ownLocalSystem.Uuid, itSystem2.Uuid};

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, true, prefix, cookie);

            //Assert
            var systemList = systems.ToList();
            Assert.Equal(2, systemList.Count);
            Assert.True(systemList.All(x => createdSystemsUuids.Contains(x.Uuid)));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Can_Include_It_Systems_From_Own_Organization_Only(OrganizationRole role)
        {
            //Arrange
            var prefix = A<string>();
            var system1Name = prefix + A<string>();
            var system2Name = prefix + A<string>();
            var system3Name = prefix + A<string>();

            var (itSystem, organization, cookie) = await CreatePrerequisitesWithUser(role, system1Name, AccessModifier.Public);
            var localSystem = await CreateSystemAsync(organization.Id, system3Name);
            await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, system2Name, AccessModifier.Public);
            var createdSystemsUuids = new[] { itSystem.Uuid, localSystem.Uuid };

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, false, prefix, cookie);

            //Assert
            var systemList = systems.ToList();
            Assert.Equal(2, systemList.Count);
            Assert.True(systemList.All(x => createdSystemsUuids.Contains(x.Uuid)));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUnusedItSystems_Returns_All_On_Whitespace_Or_Empty(OrganizationRole role)
        {
            //Arrange
            var system1Name = A<string>();
            var system2Name = A<string>();

            var (itSystem, organization, cookie) = await CreatePrerequisitesWithUser(role, system1Name, AccessModifier.Public);
            var localSystem = await CreateSystemAsync(organization.Id, system2Name);
            var createdSystemsUuids = new[] { itSystem.Uuid, localSystem.Uuid };

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, false, null, cookie);

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
            await CreateSystemAsync(organization.Id, system2Name);

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, false, system1Name, cookie);

            //Assert
            var dto = Assert.Single(systems);
            Assert.Equal(dto.Uuid, itSystem.Uuid);
            Assert.Equal(dto.Name, itSystem.Name);
        }

        [Fact, Description("Systems in use in our own organization should not be included")]
        public async Task GetUnusedItSystems_Does_Not_Include_Systems_In_Use()
        {
            //Arrange
            var system1Name = A<string>();
            var system2Name = A<string>();

            var (itSystem, organization, cookie) = await CreatePrerequisitesWithUser(OrganizationRole.GlobalAdmin, system1Name, AccessModifier.Public);
            var localSystem = await CreateSystemAsync(organization.Id, system2Name);
            await ItSystemHelper.TakeIntoUseAsync(localSystem.Id, organization.Id);

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 10, false, null, cookie);

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
            var itSystemOtherOrg = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, system2Name, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(itSystemOtherOrg.Id, organization.Id);

            //Act
            var systems = await ItSystemUsageMigrationV2Helper.GetUnusedSystemsAsync(organization.Uuid, 2, true, prefix, cookie);

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
            var (system, organization, cookie) = await CreatePrerequisitesWithUser(role);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization.Id);

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetPermissions(usage.Uuid, cookie);

            //Assert
            var command = Assert.Single(result.Commands, x => x.Id == CommandPermissionConstraints.UsageMigration.Execute);
            Assert.Equal(expectedMigrationAvailability, command.CanExecute);
        }

        [Fact]
        public async Task GetMigration_When_System_Migration_Has_No_Horizontal_Consequences()
        {
            //Arrange
            var (system, organization) = await CreatePrerequisites();
            var newSystem = await CreateSystemAsync(organization.Id, CreateName());
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization.Id);

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
            var newSystem = await CreateSystemAsync(organization.Id, CreateName());
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization.Id);

            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            await ItContractHelper.AddItSystemUsage(contract.Id, usage.Id, organization.Id);

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
            var newSystem = await CreateSystemAsync(organization.Id, CreateName());
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization.Id);

            var dpr = await DataProcessingRegistrationHelper.CreateAsync(organization.Id, CreateName());
            await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dpr.Id, usage.Id);

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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);

            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), organization.Id, AccessModifier.Public));
            var exhibit = await InterfaceExhibitHelper.CreateExhibit(fromSystem.Id, interfaceDto.Id);

            var relationDTO = await CreateSystemRelation(
                toSystemUsage.Id,
                fromSystemUsage.Id,
                A<string>(),
                exhibit.ItInterfaceId,
                null,
                null
            );

            //Act
            var result = await ItSystemUsageMigrationV2Helper.GetMigration(fromSystemUsage.Uuid, toSystem.Uuid);
            
            //Assert
            Assert.Empty(result.AffectedDataProcessingRegistrations);
            Assert.Empty(result.AffectedContracts);

            var affectedRelation = Assert.Single(result.AffectedRelations);
            Assert.Equal(relationDTO.ToUsage.Name, affectedRelation.ToSystemUsage.Name);
            Assert.Equal(relationDTO.FromUsage.Name, affectedRelation.FromSystemUsage.Name);
            Assert.Equal(relationDTO.Description, affectedRelation.Description);
            Assert.Equal(relationDTO.Interface.Name, affectedRelation.Interface.Name);
            Assert.Null(affectedRelation.Contract);
            Assert.Null(affectedRelation.FrequencyType);

            AssertFromToSystemInfo(fromSystemUsage, result, fromSystem, toSystem);
        }

        [Fact]
        public async Task GetMigration_When_System_Has_Relation_With_No_RelationInterface()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);

            await CreateSystemRelation(
                toSystemUsage.Id,
                fromSystemUsage.Id,
                A<string>(),
                null,
                null,
                null
            );

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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);
            var contract = await ItContractHelper.CreateContract(A<string>(), organization.Id);

            await CreateSystemRelation(
                toSystemUsage.Id,
                fromSystemUsage.Id,
                A<string>(),
                null,
                null,
                contract.Id
            );

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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());

            //Act and Assert (expected no content)
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, toSystem.Uuid);
        }

        [Fact]
        public async Task PostMigration_SystemUsageAssociation_In_Contract_Is_Not_Changed()
        {
            //Arrange
            var (fromSystem, organization) = await CreatePrerequisites();
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var contract = await ItContractHelper.CreateContract(A<string>(), organization.Id);
            await ItContractHelper.AddItSystemUsage(contract.Id, fromSystemUsage.Id, organization.Id);

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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);
            
            var contract = await ItContractHelper.CreateContract(A<string>(), organization.Id);
            await ItContractHelper.AddItSystemUsage(contract.Id, fromSystemUsage.Id, organization.Id);

            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), organization.Id, AccessModifier.Public));
            var exhibit = await InterfaceExhibitHelper.CreateExhibit(toSystem.Id, interfaceDto.Id);
            
            var relation = await CreateSystemRelation(
                fromSystemUsage.Id,
                toSystemUsage.Id,
                A<string>(),
                exhibit.ItInterfaceId,
                GetValidFrequencyTypeId(),
                contract.Id
            );
            
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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            
            var contract = await ItContractHelper.CreateContract(A<string>(), organization.Id);
            await ItContractHelper.AddItSystemUsage(contract.Id, fromSystemUsage.Id, organization.Id);
            
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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);
            
            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), organization.Id, AccessModifier.Public));
            var exhibit = await InterfaceExhibitHelper.CreateExhibit(toSystem.Id, interfaceDto.Id);

            var relation = await CreateSystemRelation(
                fromSystemUsage.Id,
                toSystemUsage.Id,
                A<string>(),
                exhibit.ItInterfaceId,
                null,
                null
            );

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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);
            
            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), organization.Id, AccessModifier.Public));
            var exhibit = await InterfaceExhibitHelper.CreateExhibit(toSystem.Id, interfaceDto.Id);

            var migrateToItSystem = await CreateSystemAsync(organization.Id, CreateName());

            var relation = await CreateSystemRelation(
                fromSystemUsage.Id,
                toSystemUsage.Id,
                A<string>(),
                exhibit.ItInterfaceId,
                null,
                null
            );

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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);
            
            var migrateToItSystem = await CreateSystemAsync(organization.Id, CreateName());

            var relation = await CreateSystemRelation(
                fromSystemUsage.Id,
                toSystemUsage.Id,
                A<string>(),
                null,
                null,
                null
            );

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
            var fromSystemUsage = await ItSystemHelper.TakeIntoUseAsync(fromSystem.Id, organization.Id);
            var toSystem = await CreateSystemAsync(organization.Id, CreateName());
            var toSystemUsage = await ItSystemHelper.TakeIntoUseAsync(toSystem.Id, organization.Id);
            var contract = await ItContractHelper.CreateContract(A<string>(), organization.Id);

            var migrateToItSystem = await CreateSystemAsync(organization.Id, CreateName());

            var relation = await CreateSystemRelation(
                fromSystemUsage.Id,
                toSystemUsage.Id,
                A<string>(),
                null,
                null,
                contract.Id
            );

            //Act
            await ItSystemUsageMigrationV2Helper.ExecuteMigration(fromSystemUsage.Uuid, migrateToItSystem.Uuid);

            //Assert
            await AssertRelationExists(relation, fromSystemUsage, hasContract: true);
        }

        private static void AssertFromToSystemInfo(
            ItSystemUsageDTO usage,
            ItSystemUsageMigrationV2ResponseDTO result,
            ItSystemDTO oldSystem,
            ItSystemDTO newSystem)
        {
            Assert.Equal(usage.Uuid, result.TargetUsage.Uuid);
            Assert.Equal(oldSystem.Uuid, result.FromSystem.Uuid);
            Assert.Equal(newSystem.Uuid, result.ToSystem.Uuid);
        }

        private static async Task AssertSystemUsageAssociationExistsInContract(ItContractDTO contract, ItSystemUsageDTO usage)
        {
            var contractFromServer = await ItContractHelper.GetItContract(contract.Id);

            Assert.Equal(1, contractFromServer.AssociatedSystemUsages.Count(x => x.Id == usage.Id));
        }

        private static async Task AssertRelationExists(SystemRelationDTO expectedRelation, ItSystemUsageDTO usage, bool hasInterface = false, bool hasFrequency = false, bool hasContract = false)
        {
            using var response = await SystemRelationHelper.SendGetRelationRequestAsync(usage.Id, expectedRelation.Id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var relation = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
            Assert.Equal(expectedRelation.Id, relation.Id);
            Assert.Equal(expectedRelation.Description, relation.Description);
            if (hasInterface)
            {
                AssertEqualNamedEntities(expectedRelation.Interface, relation.Interface);
            }
            else
            {
                Assert.Null(relation.Interface);
            }

            if (hasFrequency)
            {
                AssertEqualNamedEntities(expectedRelation.FrequencyType, relation.FrequencyType);
            }
            else
            {
                Assert.Null(relation.FrequencyType);
            }

            if (hasContract)
            {
                AssertEqualNamedEntities(expectedRelation.Contract, relation.Contract);
            }
            else
            {
                Assert.Null(relation.Contract);
            }
        }

        private static void AssertEqualNamedEntities(NamedEntityDTO expected, NamedEntityDTO actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
        }

        private async Task<(ItSystemDTO system, OrganizationDTO organization, Cookie cookie)> CreatePrerequisitesWithUser(OrganizationRole role, string systemName = null, AccessModifier accessModifier = AccessModifier.Local)
        {
            var (system, organization) = await CreatePrerequisites(systemName, accessModifier);
            var user = await HttpApi.CreateUserAndLogin(CreateEmail(), role, organization.Id);

            return (system, organization, user.loginCookie);
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

        private static async Task<SystemRelationDTO> CreateSystemRelation(
            int fromUsageId,
            int toUsageId,
            string description,
            int? interfaceId,
            int? frequencyTypeId,
            int? contractId)
        {
            var relationDTO = new CreateSystemRelationDTO()
            {
                FromUsageId = fromUsageId,
                ToUsageId = toUsageId,
                Description = description,
                InterfaceId = interfaceId,
                FrequencyTypeId = frequencyTypeId,
                ContractId = contractId
            };
            return await SystemRelationHelper.PostRelationAsync(relationDTO);
        }

        private string CreateName()
        {
            return nameof(ItSystemUsageMigrationApiV2Test) + A<string>();
        }

        private string CreateEmail()
        {
            return $"{A<string>()}@kitos.dk";
        }

        private static int GetValidFrequencyTypeId()
        {
            return new Random().Next(1, 4);
        }
    }
}
