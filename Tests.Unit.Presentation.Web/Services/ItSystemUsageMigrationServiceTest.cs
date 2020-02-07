using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItSystemUsageMigrationServiceTest : WithAutoFixture
    {
        private readonly ItSystemUsageMigrationService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<IItSystemUsageRepository> _systemUsageRepository;
        private readonly Mock<IItContractRepository> _itContractRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IItSystemUsageService> _itSystemUsageService;

        public ItSystemUsageMigrationServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();

            _systemUsageRepository = new Mock<IItSystemUsageRepository>();
            _itContractRepository = new Mock<IItContractRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _itSystemUsageService = new Mock<IItSystemUsageService>();

            _sut = new ItSystemUsageMigrationService(
                _authorizationContext.Object,
                _transactionManager.Object,
                Mock.Of<ILogger>(),
                _systemRepository.Object,
                _systemUsageRepository.Object,
                _itSystemUsageService.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GetUnusedItSystemsByOrganization_Throws_On_Empty_Name_Content(string nameContent)
        {
            //Arrange
            var organizationId = A<int>();

            //Act + Assert
            Assert.Throws<ArgumentException>(() => _sut.GetUnusedItSystemsByOrganization(organizationId, nameContent, 1337, A<bool>()));
        }

        [Fact]
        public void GetUnusedItSystemsByOrganization_Throws_On_NumberOfItSystems_Less_Than_1()
        {
            //Arrange
            var organizationId = A<int>();

            //Act + Assert
            Assert.Throws<ArgumentException>(() => _sut.GetUnusedItSystemsByOrganization(organizationId, A<string>(), 0, A<bool>()));
        }

        [Fact]
        public void GetUnusedItSystemsByOrganization_Returns_Forbidden_If_Organization_Access_Is_Denied()
        {
            //Arrange
            var organizationId = A<int>();
            ExpectOrganizationalAccessLevel(organizationId, OrganizationDataReadAccessLevel.None);

            //Act
            var result = _sut.GetUnusedItSystemsByOrganization(organizationId, A<string>(), 1337, A<bool>());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Theory]
        [InlineData(2, true, CrossOrganizationDataReadAccessLevel.Public, OrganizationDataReadAccessLevel.Public)]
        [InlineData(3, false, CrossOrganizationDataReadAccessLevel.All, OrganizationDataReadAccessLevel.Public)]
        [InlineData(3, false, CrossOrganizationDataReadAccessLevel.None, OrganizationDataReadAccessLevel.All)]
        public void GetUnusedItSystemsByOrganization_Returns_RequestedAmount_OrderedBy_Name(int amount, bool getPublic, CrossOrganizationDataReadAccessLevel crossOrganizationDataReadAccessLevel, OrganizationDataReadAccessLevel organizationDataReadAccess)
        {
            //Arrange
            var prefix = A<string>();
            var organizationId = A<int>();
            ExpectOrganizationalAccessLevel(organizationId, organizationDataReadAccess);
            ExpectCrossLevelOrganizationAccess(crossOrganizationDataReadAccessLevel);
            var expectedQuery = CreateExpectedQuery(getPublic, crossOrganizationDataReadAccessLevel, organizationDataReadAccess, organizationId);

            //Create double the amount of requested to check that amount is limited by requested number. 
            var resultSet = CreateItSystemSequenceWithNamePrefix(amount * 2, prefix);

            ExpectGetUnusedSystemsReturns(expectedQuery, resultSet.AsQueryable());

            //Act
            var result = _sut.GetUnusedItSystemsByOrganization(organizationId, prefix, amount, getPublic);

            //Assert + requested amount returned and in the right order
            Assert.True(result.Ok);
            var itSystems = result.Value.ToList();
            Assert.Equal(amount, itSystems.Count);
            Assert.True(resultSet.OrderBy(x => x.Name).Take(amount).SequenceEqual(itSystems));
        }

        [Fact]
        public void GetUnusedItSystemsByOrganization_Filters_By_Name()
        {
            //Arrange
            var prefix = A<string>();
            var organizationId = A<int>();
            var getPublic = A<bool>();

            var expectedQuery = CreateExpectedQuery(getPublic, CrossOrganizationDataReadAccessLevel.All, OrganizationDataReadAccessLevel.All, organizationId);

            ExpectOrganizationalAccessLevel(organizationId, expectedQuery.DataAccessLevel.CurrentOrganization);
            ExpectCrossLevelOrganizationAccess(expectedQuery.DataAccessLevel.CrossOrganizational);


            //Create double the amount of requested to check that amount is limited by requested number. 
            var resultSet = CreateItSystemSequenceWithNamePrefix(2, prefix);
            resultSet.Last().Name = A<string>(); //Last one does not match naming criterion

            ExpectGetUnusedSystemsReturns(expectedQuery, resultSet.AsQueryable());

            //Act
            var result = _sut.GetUnusedItSystemsByOrganization(organizationId, prefix, 2, getPublic);

            //Assert Only the one that matches the naming criterion is returned
            Assert.True(result.Ok);
            var itSystem = Assert.Single(result.Value);
            Assert.Equal(resultSet.First().Id, itSystem?.Id);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Forbidden_If_Migration_Is_UnAuthorized()
        {
            //Arrange
            ExpectAllowSystemMigrationReturns(false);

            //Act
            var result = _sut.GetSystemUsageMigration(A<int>(), A<int>());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Forbidden_If_Read_Access_To_SystemUsage_Is_Unauthorized()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            ExpectAllowSystemMigrationReturns(true);
            ExpectGetSystemUsageReturns(systemUsage.Id, systemUsage);
            ExpectAllowReadsReturns(systemUsage, false);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, A<int>());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Forbidden_If_Read_Access_To_System_Is_Unauthorized()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowSystemMigrationReturns(true);
            ExpectGetSystemUsageReturns(systemUsage.Id, systemUsage);
            ExpectAllowReadsReturns(systemUsage, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, false);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_BadInput_If_SystemUsage_Does_Not_Exist()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowSystemMigrationReturns(true);
            ExpectGetSystemUsageReturns(systemUsage.Id, null);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_BadInput_If_System_Does_Not_Exist()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowSystemMigrationReturns(true);
            ExpectGetSystemUsageReturns(systemUsage.Id, systemUsage);
            ExpectAllowReadsReturns(systemUsage, true);
            ExpectGetSystemReturns(system.Id, null);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_From_And_To_Mappings()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var newSystem = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, newSystem);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, newSystem.Id);

            //Assert OK and correct marking of contract WITH and WITHOUT associations
            Assert.True(result.Ok);
            var migration = result.Value;
            Assert.Empty(migration.AffectedContracts);
            Assert.Empty(migration.AffectedProjects);
            Assert.Empty(migration.AffectedSystemRelations);
            Assert.Equal(systemUsage, migration.SystemUsage);
            Assert.Equal(systemUsage.ItSystem, migration.FromItSystem);
            Assert.Equal(newSystem, migration.ToItSystem);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_Affected_Projects()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, system);
            systemUsage.ItProjects = new List<ItProject> { CreateProject(), CreateProject() };

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, system.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            Assert.Equal(2, migration.AffectedProjects.Count);
            Assert.True(systemUsage.ItProjects.SequenceEqual(migration.AffectedProjects));
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_Affected_Contracts()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, system);

            systemUsage = AddContractToSystemUsage(systemUsage,
                new List<ItContract>() { CreateItContract(), CreateItContract() });

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, system.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            Assert.Equal(2, migration.AffectedContracts.Count);
            Assert.True(systemUsage.Contracts.Select(x => x.ItContract).SequenceEqual(migration.AffectedContracts));
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_No_Affected_Relation_From_UsageRelations()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            var relation = new SystemRelation(fromSystemUsage) { ToSystemUsageId = toSystemUsage.Id };
            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);
            var usageRelations = new List<SystemRelation>
            {
                relation
            };
            var usedByRelations = new List<SystemRelation>();
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);

            //Act
            var result = _sut.GetSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            Assert.Empty(migration.AffectedSystemRelations);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_Affected_Relation_From_UsedByRelation_With_Interface()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            var relationInterface = CreateInterface();
            var relation = new SystemRelation(fromSystemUsage)
            {
                ToSystemUsageId = toSystemUsage.Id,
                RelationInterfaceId = relationInterface.Id,
                RelationInterface = relationInterface
            };

            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);
            var usageRelations = new List<SystemRelation>();
            var usedByRelations = new List<SystemRelation>
            {
                relation
            };
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);

            //Act
            var result = _sut.GetSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            Assert.Equal(1, migration.AffectedSystemRelations.Count);
            Assert.Equal(relation.Id, migration.AffectedSystemRelations.First().Id);
            Assert.Equal(relationInterface.Id, migration.AffectedSystemRelations.First().RelationInterfaceId);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_No_Affected_Relation_From_UsedByRelation_With_Contract()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            var contract = CreateItContract();
            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);
            var usageRelations = new List<SystemRelation>();
            var usedByRelations = new List<SystemRelation>
            {
                new SystemRelation(fromSystemUsage) {ToSystemUsageId = toSystemUsage.Id, AssociatedContractId = contract.Id, AssociatedContract = contract}
            };
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);

            //Act
            var result = _sut.GetSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            Assert.Empty(migration.AffectedSystemRelations);
        }

        [Fact]
        public void ExecuteMigration_Returns_Forbidden_If_Migration_Is_UnAuthorized()
        {
            //Arrange
            ExpectAllowSystemMigrationReturns(false);

            //Act
            var result = _sut.ExecuteSystemUsageMigration(A<int>(), A<int>());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void ExecuteMigration_Returns_Forbidden_If_Modification_Of_ItSystemUsage_UnAuthorized()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, system);
            ExpectBeginTransaction();
            ExpectAllowModifyReturns(systemUsage, false);

            //Act
            var result = _sut.ExecuteSystemUsageMigration(systemUsage.Id, system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void ExecuteMigration_Returns_Ok_If_Existing_System_Is_Target()
        {
            //Arrange
            var systemUsage = CreateSystemUsage();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, systemUsage.ItSystem);
            var transaction = ExpectBeginTransaction();
            ExpectAllowModifyReturns(systemUsage, true);

            //Act
            var result = _sut.ExecuteSystemUsageMigration(systemUsage.Id, systemUsage.ItSystem.Id);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void ExecuteMigration_Returns_Ok_And_Deletes_Access_Types()
        {
            //Arrange
            var newSystem = CreateSystem();
            var systemUsage = CreateSystemUsage();

            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, newSystem);
            var transaction = ExpectBeginTransaction();
            ExpectAllowModifyReturns(systemUsage, true);
            systemUsage.AccessTypes = new List<AccessType> { new AccessType(), new AccessType() }; //Set access types

            //Act
            var result = _sut.ExecuteSystemUsageMigration(systemUsage.Id, newSystem.Id);

            //Assert - Check that access types have been removed from the system usage
            Assert.True(result.Ok);
            Assert.Empty(systemUsage.AccessTypes);
            VerifySystemMigrationCommitted(systemUsage, newSystem, transaction);
        }

        [Fact]
        public void ExecuteMigration_Returns_Ok_And_Resets_RelationInterface_From_UsedByRelation_With_Interface()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            var relationInterface = CreateInterface();
            var relation = new SystemRelation(fromSystemUsage)
            {
                ToSystemUsageId = toSystemUsage.Id,
                RelationInterfaceId = relationInterface.Id,
                RelationInterface = relationInterface
            };

            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);
            ExpectAllowModifyReturns(fromSystemUsage, true);
            var usageRelations = new List<SystemRelation>();
            var usedByRelations = new List<SystemRelation>
            {
                relation
             };
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);
            ExpectSystemUsageServiceToRemoveRelationInterface(usedByRelations.First(), true);
            var transaction = ExpectBeginTransaction();

            //Act
            var result = _sut.ExecuteSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            VerifyModifyRelationCalled(relation, Times.Once());
            Assert.Equal(relation.Id, migration.UsedByRelations.First().Id);
            VerifySystemMigrationCommitted(fromSystemUsage, migrateToSystem, transaction);
        }

        [Fact]
        public void ExecuteMigration_Returns_Ok_With_No_RelationInterfaces_Reset_When_Own_Relation()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);
            ExpectAllowModifyReturns(fromSystemUsage, true);
            var relation = new SystemRelation(fromSystemUsage)
            {
                ToSystemUsageId = toSystemUsage.Id,
            };
            var usageRelations = new List<SystemRelation>
            {
                relation
            };
            var usedByRelations = new List<SystemRelation>();
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);
            var transaction = ExpectBeginTransaction();

            //Act
            var result = _sut.ExecuteSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            VerifyModifyRelationCalled(relation, Times.Never());
            Assert.Equal(migration.UsageRelations.First().Id, usageRelations.First().Id);
            VerifySystemMigrationCommitted(fromSystemUsage, migrateToSystem, transaction);
        }

        [Fact]
        public void ExecuteMigration_Returns_Ok_With_No_RelationInterfaces_Reset_When_Others_Relation_Has_Only_Contract()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            var contract = CreateItContract();
            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);
            ExpectAllowModifyReturns(fromSystemUsage, true);
            var relation = new SystemRelation(fromSystemUsage)
            {
                ToSystemUsageId = toSystemUsage.Id,
                AssociatedContractId = contract.Id,
                AssociatedContract = contract
            };
            var usageRelations = new List<SystemRelation>();
            var usedByRelations = new List<SystemRelation>
            {
                relation
            };
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);
            var transaction = ExpectBeginTransaction();

            //Act
            var result = _sut.ExecuteSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            VerifyModifyRelationCalled(relation, Times.Never());
            Assert.Equal(migration.UsedByRelations.First().Id, usedByRelations.First().Id);
            VerifySystemMigrationCommitted(fromSystemUsage, migrateToSystem, transaction);
        }

        [Fact]
        public void ExecuteMigration_Returns_Ok_With_No_RelationInterfaces_Reset_When_Own_Relation_Has_Interface()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            var relationInterface = CreateInterface();
            var relation = new SystemRelation(fromSystemUsage)
            {
                ToSystemUsageId = toSystemUsage.Id,
                RelationInterfaceId = relationInterface.Id,
                RelationInterface = relationInterface
            };
            ExpectAllowModifyReturns(fromSystemUsage, true);
            var usageRelations = new List<SystemRelation>
            {
                relation
            };
            var usedByRelations = new List<SystemRelation>();
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);
            var transaction = ExpectBeginTransaction();
            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);

            //Act
            var result = _sut.ExecuteSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.True(result.Ok);
            var migration = result.Value;
            VerifyModifyRelationCalled(relation, Times.Never());
            Assert.Equal(relation.Id, migration.UsageRelations.First().Id);
            VerifySystemMigrationCommitted(fromSystemUsage, migrateToSystem, transaction);
        }

        [Fact]
        public void ExecuteMigration_Returns_UnknownError_If_Deletion_Of_Relation_Interface_Fails()
        {
            //Arrange
            var fromSystemUsage = CreateSystemUsage();
            var migrateToSystem = CreateSystem();
            var toSystemUsage = CreateSystemUsage();
            var relationInterface = CreateInterface();
            var relation = new SystemRelation(fromSystemUsage)
            {
                ToSystemUsageId = toSystemUsage.Id,
                RelationInterfaceId = relationInterface.Id,
                RelationInterface = relationInterface
            };

            var usageRelations = new List<SystemRelation>();
            var usedByRelations = new List<SystemRelation>
            {
                relation
            };
            fromSystemUsage = AddRelationsToSystemUsage(fromSystemUsage, usageRelations, usedByRelations);
            ExpectSystemUsageServiceToRemoveRelationInterface(usedByRelations.First(), false);
            ExpectAllowedGetMigration(fromSystemUsage.Id, fromSystemUsage, migrateToSystem);
            ExpectAllowModifyReturns(fromSystemUsage, true);
            var transaction = ExpectBeginTransaction();

            //Act
            var result = _sut.ExecuteSystemUsageMigration(fromSystemUsage.Id, migrateToSystem.Id);

            //Assert
            Assert.False(result.Ok);
            VerifyTransactionRollback(transaction);
        }

        private static void VerifyTransactionRollback(Mock<IDatabaseTransaction> transaction)
        {
            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        private void VerifyModifyRelationCalled(SystemRelation relation, Times numberOfTimesCalled)
        {
            _itSystemUsageService.Verify(x => x.ModifyRelation(
                relation.FromSystemUsageId,
                relation.Id,
                relation.ToSystemUsageId,
                relation.Description,
                relation.Reference,
                null,
                relation.AssociatedContractId,
                relation.UsageFrequencyId), numberOfTimesCalled);
        }


        private void VerifySystemMigrationCommitted(ItSystemUsage systemUsage, ItSystem newSystem, Mock<IDatabaseTransaction> transaction)
        {
            //Verify update call to system usage with changed system id that points to the new system
            _systemUsageRepository
                .Verify(x => x.Update(It.Is<ItSystemUsage>(usage => usage == systemUsage && systemUsage.ItSystemId == newSystem.Id)),
                Times.Once);

            //Verify that transaction is committed
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        private Mock<IDatabaseTransaction> ExpectBeginTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable))
                .Returns(transaction.Object);
            return transaction;
        }

        private void ExpectAllowModifyReturns(ItSystemUsage systemUsage, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(systemUsage)).Returns(value);
        }

        private ItSystemUsage AddRelationsToSystemUsage(ItSystemUsage usage, ICollection<SystemRelation> usageRelations, ICollection<SystemRelation> usedByRelations)
        {
            usage.UsageRelations = usageRelations;
            usage.UsedByRelations = usedByRelations;
            return usage;
        }

        private ItSystemUsage AddContractToSystemUsage(ItSystemUsage usage, IEnumerable<ItContract> itContracts)
        {
            var contractSystemUsages = new List<ItContractItSystemUsage>();
            foreach (var contract in itContracts)
            {
                var contractSystemUsage = CreateContractItSystemUsage(usage.Id);
                contractSystemUsage.ItContract = contract;
                contractSystemUsages.Add(contractSystemUsage);
            }

            usage.Contracts = contractSystemUsages;
            return usage;
        }

        private void ExpectAllowedGetMigration(int usageId, ItSystemUsage systemUsage, ItSystem system)
        {
            ExpectAllowSystemMigrationReturns(true);
            ExpectGetSystemUsageReturns(usageId, systemUsage);
            ExpectAllowReadsReturns(systemUsage, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, true);
        }

        private ItContract CreateItContract(
            IEnumerable<int> idsOfAssociatedSystemUsages = null,
            IEnumerable<int> idsOfSystemUsagesInInterfaceUsages = null,
            IEnumerable<int> idsOfSystemUsagesInInterfaceExposures = null)
        {
            return new ItContract
            {
                Id = A<int>(),
                AssociatedSystemUsages = CreateAssociatedSystemUsages(idsOfAssociatedSystemUsages ?? Enumerable.Empty<int>()),
                AssociatedInterfaceUsages = CreateAssociatedInterfaceUsages(idsOfSystemUsagesInInterfaceUsages ?? Enumerable.Empty<int>()),
                AssociatedInterfaceExposures = CreateAssociatedInterfaceExposures(idsOfSystemUsagesInInterfaceExposures ?? Enumerable.Empty<int>()),
            };
        }

        private List<ItInterfaceExhibitUsage> CreateAssociatedInterfaceExposures(IEnumerable<int> systemUsageIds)
        {
            return systemUsageIds.Select(usageId =>
                new ItInterfaceExhibitUsage
                {
                    ItSystemUsageId = usageId,
                    ItContractId = A<int>(),
                    ItInterfaceExhibitId = A<int>()
                }).ToList();
        }

        private List<ItInterfaceUsage> CreateAssociatedInterfaceUsages(IEnumerable<int> systemUsageIds)
        {
            return systemUsageIds.Select(usageId => new ItInterfaceUsage
            {
                ItSystemUsageId = usageId,
                ItSystemId = A<int>(),
                ItInterfaceId = A<int>(),
                InfrastructureId = A<int>(),
                IsWishedFor = A<bool>()
            }).ToList();
        }

        private static List<ItContractItSystemUsage> CreateAssociatedSystemUsages(IEnumerable<int> systemUsageIds)
        {
            return systemUsageIds.Select(CreateContractItSystemUsage).ToList();
        }

        private static ItContractItSystemUsage CreateContractItSystemUsage(int itSystemUsageId)
        {
            return new ItContractItSystemUsage { ItSystemUsageId = itSystemUsageId };
        }

        private void ExpectGetSystemReturns(int systemId, ItSystem system)
        {
            _systemRepository.Setup(x => x.GetSystem(systemId)).Returns(system);
        }

        private void ExpectAllowReadsReturns(IEntity systemUsage, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(systemUsage)).Returns(value);
        }

        private void ExpectGetSystemUsageReturns(int usageId, ItSystemUsage systemUsage)
        {
            _systemUsageRepository.Setup(x => x.GetSystemUsage(usageId)).Returns(systemUsage);
        }

        private void ExpectAllowSystemMigrationReturns(bool value)
        {
            _authorizationContext.Setup(x => x.HasPermission(It.IsAny<SystemUsageMigrationPermission>())).Returns(value);
        }

        private void ExpectSystemUsageServiceToRemoveRelationInterface(SystemRelation relation, bool shouldSucceed)
        {
            var newRelation = new SystemRelation(relation.FromSystemUsage)
            {
                Id = relation.Id,
                Description = relation.Description,
                AssociatedContract = relation.AssociatedContract,
                AssociatedContractId = relation.AssociatedContractId,
                Reference = relation.Reference,
                ToSystemUsage = relation.ToSystemUsage,
                ToSystemUsageId = relation.ToSystemUsageId,
                UsageFrequency = relation.UsageFrequency,
                UsageFrequencyId = relation.UsageFrequencyId,
                RelationInterfaceId = null,
                RelationInterface = null
            };

            _itSystemUsageService
                .Setup(x => x.ModifyRelation(
                    relation.FromSystemUsageId,
                    relation.Id,
                    relation.ToSystemUsageId,
                    relation.Description,
                    relation.Reference,
                    null,
                    relation.AssociatedContractId,
                    relation.UsageFrequencyId))
                .Returns(
                    shouldSucceed 
                    ? Result<SystemRelation, OperationError>.Success(newRelation) 
                    : Result<SystemRelation, OperationError>.Failure(new OperationError(OperationFailure.UnknownError)));


        }

        private static List<ItSystem> CreateItSystemSequenceWithNamePrefix(int amount, string prefix)
        {
            var resultSet = Enumerable.Range(0, amount).Select(id => new ItSystem { Id = id, Name = prefix + "_" + id })
                .Reverse().ToList();
            return resultSet;
        }

        private void ExpectGetUnusedSystemsReturns(OrganizationDataQueryParameters expectedQuery, IQueryable<ItSystem> expectedResult)
        {
            _systemRepository.Setup(x => x.GetUnusedSystems(expectedQuery)).Returns(expectedResult);
        }

        private static OrganizationDataQueryParameters CreateExpectedQuery(bool getPublic, CrossOrganizationDataReadAccessLevel crossOrganizationDataReadAccessLevel, OrganizationDataReadAccessLevel organizationDataReadAccess, int organizationId)
        {
            return new OrganizationDataQueryParameters(organizationId,
                getPublic
                    ? OrganizationDataQueryBreadth.IncludePublicDataFromOtherOrganizations
                    : OrganizationDataQueryBreadth.TargetOrganization,
                new DataAccessLevel(crossOrganizationDataReadAccessLevel, organizationDataReadAccess));
        }

        private void ExpectCrossLevelOrganizationAccess(
            CrossOrganizationDataReadAccessLevel crossOrganizationDataReadAccessLevel)
        {
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(crossOrganizationDataReadAccessLevel);
        }

        private void ExpectOrganizationalAccessLevel(int organizationId, OrganizationDataReadAccessLevel accessLevel)
        {
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(accessLevel);
        }

        private ItSystemUsage CreateSystemUsage()
        {
            var itSystem = CreateSystem();
            return new ItSystemUsage
            {
                Id = A<int>(),
                ItProjects = new List<ItProject>(),
                ItSystem = itSystem,
                ItSystemId = itSystem.Id
            };
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem { Id = A<int>() };
        }

        private ItProject CreateProject()
        {
            return new ItProject() { Id = A<int>() };
        }

        private ItInterface CreateInterface()
        {
            return new ItInterface() { Id = A<int>() };
        }
    }
}
