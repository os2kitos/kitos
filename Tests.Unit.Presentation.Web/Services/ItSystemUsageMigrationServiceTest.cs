using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Moq;
using Serilog;
using Tests.Unit.Presentation.Web.Helpers;
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

        public ItSystemUsageMigrationServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();

            _systemUsageRepository = new Mock<IItSystemUsageRepository>();
            _itContractRepository = new Mock<IItContractRepository>();
            _sut = new ItSystemUsageMigrationService(
                _authorizationContext.Object,
                null,
                Mock.Of<ILogger>(),
                _systemRepository.Object,
                _systemUsageRepository.Object,
                _itContractRepository.Object,
                null,
                null);
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
            Assert.Equal(OperationResult.Forbidden, result.Status);
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
            Assert.Equal(OperationResult.Ok, result.Status);
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
            Assert.Equal(OperationResult.Ok, result.Status);
            var itSystem = Assert.Single(result.Value);
            Assert.Equal(resultSet.First().Id, itSystem.Id);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Forbidden_If_Migration_Is_UnAuthorized()
        {
            //Arrange
            ExpectAllowSystemMigrationReturns(false);

            //Act
            var result = _sut.GetSystemUsageMigration(A<int>(), A<int>());

            //Assert
            Assert.Equal(OperationResult.Forbidden, result.Status);
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
            Assert.Equal(OperationResult.Forbidden, result.Status);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Forbidden_If_Read_Access_To_System_Is_Unauthorized()
        {
            //Arrange
            var systemId = A<int>();
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowSystemMigrationReturns(true);
            ExpectGetSystemUsageReturns(systemUsage.Id, systemUsage);
            ExpectAllowReadsReturns(systemUsage, true);
            ExpectGetSystemReturns(systemId, system);
            ExpectAllowReadsReturns(system, false);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, systemId);

            //Assert
            Assert.Equal(OperationResult.Forbidden, result.Status);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_From_And_To_Mappings()
        {
            //Arrange
            var newSystemId = A<int>();
            var systemUsage = CreateSystemUsage();
            var newSystem = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, newSystemId, newSystem);
            ExpectGetContractsBySystemUsageReturns(systemUsage.Id, Enumerable.Empty<ItContract>());

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, newSystemId);

            //Assert OK and correct marking of contract WITH and WITHOUT associations
            Assert.Equal(OperationResult.Ok, result.Status);
            var migration = result.Value;
            Assert.Empty(migration.AffectedContracts);
            Assert.Empty(migration.AffectedProjects);
            Assert.Equal(systemUsage, migration.SystemUsage);
            Assert.Equal(systemUsage.ItSystem, migration.FromItSystem);
            Assert.Equal(newSystem, migration.ToItSystem);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_ContractThatHasSystemAssociation()
        {
            //Arrange
            var newSystemId = A<int>();
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, newSystemId, system);

            var contractWithAssociatedUsage = CreateItContract(new[] { systemUsage.Id, A<int>() });
            var contractWithNoMigrationEffects = CreateItContract();

            var itContracts = new[]
            {
                //A contract with associated system usages
                contractWithAssociatedUsage,
                //A contract with nothing interesting
                contractWithNoMigrationEffects,
            };
            ExpectGetContractsBySystemUsageReturns(systemUsage.Id, itContracts);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, newSystemId);

            //Assert OK and correct marking of contract WITH and WITHOUT associations
            Assert.Equal(OperationResult.Ok, result.Status);
            var migration = result.Value;
            Assert.Equal(2, migration.AffectedContracts.Count);

            var migrationWithAssociations = Assert.Single(migration.AffectedContracts.Where(x => x.Contract.Id == contractWithAssociatedUsage.Id));
            Assert.True(migrationWithAssociations.SystemAssociatedInContract);

            var migrationWithOutAssociations = Assert.Single(migration.AffectedContracts.Where(x => x.Contract.Id == contractWithNoMigrationEffects.Id));
            Assert.False(migrationWithOutAssociations.SystemAssociatedInContract);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_ContractThatHasInterfaceUsage()
        {
            //Arrange
            var newSystemId = A<int>();
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, newSystemId, system);

            var contractWithInterfaceUsage = CreateItContract(idsOfSystemUsagesInInterfaceUsages: new[] { systemUsage.Id, A<int>() });
            var contractWithNoMigrationEffects = CreateItContract(idsOfSystemUsagesInInterfaceUsages: Many<int>());

            var itContracts = new[]
            {
                //A contract with associated system usages
                contractWithInterfaceUsage,
                //A contract with nothing interesting
                contractWithNoMigrationEffects,
            };
            ExpectGetContractsBySystemUsageReturns(systemUsage.Id, itContracts);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, newSystemId);

            //Assert
            Assert.Equal(OperationResult.Ok, result.Status);
            var migration = result.Value;
            Assert.Equal(2, migration.AffectedContracts.Count);

            var migrationWithAssociations = Assert.Single(migration.AffectedContracts.Where(x => x.Contract.Id == contractWithInterfaceUsage.Id));
            var interfaceUsage = Assert.Single(migrationWithAssociations.AffectedInterfaceUsages);
            Assert.Equal(systemUsage.Id, interfaceUsage.ItSystemUsageId);

            var migrationWithOutAssociations = Assert.Single(migration.AffectedContracts.Where(x => x.Contract.Id == contractWithNoMigrationEffects.Id));
            Assert.Empty(migrationWithOutAssociations.AffectedInterfaceUsages);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_ContractThatHasInterfaceExposuresThatMustBeDeleted()
        {
            //Arrange
            var newSystemId = A<int>();
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, newSystemId, system);

            var contractWithInterfaceUsage = CreateItContract(idsOfSystemUsagesInInterfaceExposures: new[] { systemUsage.Id, A<int>() });
            var contractWithNoMigrationEffects = CreateItContract(idsOfSystemUsagesInInterfaceExposures: Many<int>());

            var itContracts = new[]
            {
                //A contract with associated system usages
                contractWithInterfaceUsage,
                //A contract with nothing interesting
                contractWithNoMigrationEffects,
            };
            ExpectGetContractsBySystemUsageReturns(systemUsage.Id, itContracts);

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, newSystemId);

            //Assert
            Assert.Equal(OperationResult.Ok, result.Status);
            var migration = result.Value;
            Assert.Equal(2, migration.AffectedContracts.Count);

            var migrationWithAssociations = Assert.Single(migration.AffectedContracts.Where(x => x.Contract.Id == contractWithInterfaceUsage.Id));
            var exhibitUsage = Assert.Single(migrationWithAssociations.ExhibitUsagesToBeDeleted);
            Assert.Equal(systemUsage.Id, exhibitUsage.ItSystemUsageId);

            var migrationWithOutAssociations = Assert.Single(migration.AffectedContracts.Where(x => x.Contract.Id == contractWithNoMigrationEffects.Id));
            Assert.Empty(migrationWithOutAssociations.ExhibitUsagesToBeDeleted);
        }

        [Fact]
        public void GetSystemUsageMigration_Returns_Ok_With_Affected_Projects()
        {
            //Arrange
            var newSystemId = A<int>();
            var systemUsage = CreateSystemUsage();
            var system = CreateSystem();
            ExpectAllowedGetMigration(systemUsage.Id, systemUsage, newSystemId, system);
            ExpectGetContractsBySystemUsageReturns(systemUsage.Id, Enumerable.Empty<ItContract>());
            systemUsage.ItProjects = new List<ItProject> { CreateProject(), CreateProject() };

            //Act
            var result = _sut.GetSystemUsageMigration(systemUsage.Id, newSystemId);

            //Assert
            Assert.Equal(OperationResult.Ok, result.Status);
            var migration = result.Value;
            Assert.Equal(2, migration.AffectedProjects.Count);
            Assert.True(systemUsage.ItProjects.SequenceEqual(migration.AffectedProjects));
        }

        private void ExpectGetContractsBySystemUsageReturns(int usageId, IEnumerable<ItContract> itContracts)
        {
            _itContractRepository.Setup(x => x.GetBySystemUsageAssociation(usageId)).Returns(itContracts.AsQueryable());
        }

        private void ExpectAllowedGetMigration(int usageId, ItSystemUsage systemUsage, int systemId, ItSystem system)
        {
            ExpectAllowSystemMigrationReturns(true);
            ExpectGetSystemUsageReturns(usageId, systemUsage);
            ExpectAllowReadsReturns(systemUsage, true);
            ExpectGetSystemReturns(systemId, system);
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

        private static List<ItInterfaceExhibitUsage> CreateAssociatedInterfaceExposures(IEnumerable<int> systemUsageIds)
        {
            return systemUsageIds.Select(usageId => new ItInterfaceExhibitUsage { ItSystemUsageId = usageId }).ToList();
        }

        private static List<ItInterfaceUsage> CreateAssociatedInterfaceUsages(IEnumerable<int> systemUsageIds)
        {
            return systemUsageIds.Select(usageId => new ItInterfaceUsage { ItSystemUsageId = usageId }).ToList();
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
            _authorizationContext.Setup(x => x.AllowSystemUsageMigration()).Returns(value);
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
            return new ItSystemUsage
            {
                Id = A<int>(),
                ItProjects = new List<ItProject>(),
                ItSystem = CreateSystem()
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
    }
}
