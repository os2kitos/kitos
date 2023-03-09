using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Shared;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageMigrationServiceAdapterTest : WithAutoFixture
    {
        private readonly Mock<IEntityIdentityResolver> _identityResolver;
        private readonly Mock<IItSystemUsageMigrationService> _systemUsageMigrationService;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IOrganizationService> _organizationService;

        private readonly ItSystemUsageMigrationServiceAdapter _sut;

        public ItSystemUsageMigrationServiceAdapterTest()
        {
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _systemUsageMigrationService = new Mock<IItSystemUsageMigrationService>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _organizationService = new Mock<IOrganizationService>();

            _sut = new ItSystemUsageMigrationServiceAdapter(
                _identityResolver.Object,
                _systemUsageMigrationService.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _organizationService.Object);
        }

        [Fact]
        public void Can_GetMigration()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();
            var systemId = A<int>();

            var expectedResult = CreateItSystemUsageMigration(usageUuid, systemUuid);

            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, systemId);
            ExpectGetSystemUsageMigrationReturns(usageId, systemId, expectedResult);
            ExpectAllowReads(expectedResult.SystemUsage, true);

            //Act
            var result = _sut.GetMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Ok);
            var resultValue = result.Value;
            Assert.Equal(expectedResult.SystemUsage.Uuid, resultValue.SystemUsage.Uuid);
            Assert.Equal(expectedResult.ToItSystem.Uuid, resultValue.ToItSystem.Uuid);
        }

        [Fact]
        public void GetMigration_Returns_Forbidden_When_Not_Allowed_To_Read()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();
            var systemId = A<int>();

            var expectedResult = CreateItSystemUsageMigration(usageUuid, systemUuid);

            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, systemId);
            ExpectGetSystemUsageMigrationReturns(usageId, systemId, expectedResult);
            ExpectAllowReads(expectedResult.SystemUsage, false);

            //Act
            var result = _sut.GetMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetMigration_Returns_Error_When_GetSystemUsageMigration_Fails()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();
            var systemId = A<int>();

            var expectedResult = A<OperationError>();

            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, systemId);
            ExpectGetSystemUsageMigrationReturns(usageId, systemId, expectedResult);

            //Act
            var result = _sut.GetMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedResult.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void GetMigration_Returns_NotFound_When_SystemId_Was_NotFound()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();


            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, Maybe<int>.None);

            //Act
            var result = _sut.GetMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetMigration_Returns_NotFound_When_SystemUsageId_Was_NotFound()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var systemUuid = A<Guid>();


            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, Maybe<int>.None);

            //Act
            var result = _sut.GetMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_ExecuteMigration()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();
            var systemId = A<int>();

            var expectedResult = new ItSystemUsage{Uuid = usageUuid};

            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, systemId);
            ExpectExecuteSystemUsageMigrationReturns(usageId, systemId, expectedResult);
            ExpectAllowModify(expectedResult, true);
            var transaction = ExpectTransactionBegins();

            //Act
            var result = _sut.ExecuteMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Ok);
            var resultValue = result.Value;
            Assert.Equal(expectedResult.Uuid, resultValue.Uuid);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void ExecuteMigration_Returns_Forbidden_When_Not_Allowed_To_Modify()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();
            var systemId = A<int>();

            var expectedResult = new ItSystemUsage{Uuid = usageUuid};

            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, systemId);
            ExpectExecuteSystemUsageMigrationReturns(usageId, systemId, expectedResult);
            ExpectAllowModify(expectedResult, false);
            var transaction = ExpectTransactionBegins();

            //Act
            var result = _sut.ExecuteMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        [Fact]
        public void ExecuteMigration_Returns_Error_When_ExecuteSystemUsageMigration_Fails()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();
            var systemId = A<int>();

            var expectedResult = A<OperationError>();

            ExpectTransactionBegins();
            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, systemId);
            ExpectExecuteSystemUsageMigrationReturns(usageId, systemId, expectedResult);

            //Act
            var result = _sut.ExecuteMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedResult.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void ExecuteMigration_Returns_NotFound_When_SystemId_NotFound()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var usageId = A<int>();
            var systemUuid = A<Guid>();
            
            ExpectTransactionBegins();
            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, usageId);
            ExpectResolveIdReturns<ItSystem>(systemUuid, Maybe<int>.None);

            //Act
            var result = _sut.ExecuteMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void ExecuteMigration_Returns_NotFound_When_SystemUsageId_NotFound()
        {
            //Arrange
            var usageUuid = A<Guid>();
            var systemUuid = A<Guid>();
            
            ExpectTransactionBegins();
            ExpectResolveIdReturns<ItSystemUsage>(usageUuid, Maybe<int>.None);

            //Act
            var result = _sut.ExecuteMigration(usageUuid, systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetUnusedItSystemsByOrganization()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var organizationId = A<int>();
            var numberOfItSystems = A<int>();
            var getPublicFromTheOrganization = A<bool>();
            var conditions = new QueryByPartOfName<ItSystem>(A<string>());

            var expectedResult = new List<ItSystem> {new(), new()};

            ExpectGetOrganizationReturns(organizationUuid, new Organization{Id = organizationId});
            ExpectGetUnusedItSystemsByOrganizationQuery(organizationId, numberOfItSystems, getPublicFromTheOrganization, conditions, Result<IQueryable<ItSystem>, OperationError>.Success(expectedResult.AsQueryable()));

            //Act
            var result = _sut.GetUnusedItSystemsByOrganization(organizationUuid, numberOfItSystems, getPublicFromTheOrganization, conditions);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public void GetUnusedItSystemsByOrganization_Returns_Error_When_Get_Failed()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var organizationId = A<int>();
            var numberOfItSystems = A<int>();
            var getPublicFromTheOrganization = A<bool>();
            var conditions = new QueryByPartOfName<ItSystem>(A<string>());

            var expectedResult = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization{Id = organizationId});
            ExpectGetUnusedItSystemsByOrganizationQuery(organizationId, numberOfItSystems, getPublicFromTheOrganization, conditions, expectedResult);

            //Act
            var result = _sut.GetUnusedItSystemsByOrganization(organizationUuid, numberOfItSystems, getPublicFromTheOrganization, conditions);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedResult.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void GetUnusedItSystemsByOrganization_Returns_NotFound_When_Id_Is_Not_Resolved()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var numberOfItSystems = A<int>();
            var getPublicFromTheOrganization = A<bool>();
            var conditions = new QueryByPartOfName<ItSystem>(A<string>());

            var expectedResult = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, expectedResult);

            //Act
            var result = _sut.GetUnusedItSystemsByOrganization(organizationUuid, numberOfItSystems, getPublicFromTheOrganization, conditions);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedResult.FailureType, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_GetCommandPermissions(bool canExecute)
        {
            //Arrange
            ExpectCanExecuteMigration(canExecute);

            //Act
            var result = _sut.GetCommandPermissions();

            //Assert
            var executeCommand = Assert.Single(result, x => x.Id == CommandPermissionConstraints.UsageMigration.Execute);
            Assert.Equal(canExecute, executeCommand.CanExecute);
        }

        private static ItSystemUsageMigration CreateItSystemUsageMigration(Guid fromUsageUuid, Guid toSystemUuid)
        {
            return new ItSystemUsageMigration(new ItSystemUsage{Uuid = fromUsageUuid}, new ItSystem(),
                new ItSystem{Uuid = toSystemUuid}, new List<ItContract>(), new List<SystemRelation>(),
                new List<DataProcessingRegistration>()
            );
        }

        private void ExpectAllowReads(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowReads(entity)).Returns(result);
        }

        private void ExpectAllowModify(IEntity entity, bool result)
        {
            _authorizationContext.Setup(x => x.AllowModify(entity)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid uuid, Result<Organization, OperationError> result, OrganizationDataReadAccessLevel? accessLevel = null)
        {
            _organizationService.Setup(x => x.GetOrganization(uuid, accessLevel)).Returns(result);
        }

        private Mock<IDatabaseTransaction> ExpectTransactionBegins()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private void ExpectCanExecuteMigration(bool result)
        {
            _systemUsageMigrationService.Setup(x => x.CanExecuteMigration()).Returns(result);
        }

        private void ExpectExecuteSystemUsageMigrationReturns(int usageId, int systemId, Result<ItSystemUsage, OperationError> result)
        {
            _systemUsageMigrationService.Setup(x => x.ExecuteSystemUsageMigration(usageId, systemId)).Returns(result);
        }

        private void ExpectGetSystemUsageMigrationReturns(int usageId, int systemId, Result<ItSystemUsageMigration, OperationError> result)
        {
            _systemUsageMigrationService.Setup(x => x.GetSystemUsageMigration(usageId, systemId)).Returns(result);
        }

        private void ExpectGetUnusedItSystemsByOrganizationQuery(int orgId, int numberOfItSystems, bool getPublic, IDomainQuery<ItSystem> condition, Result<IQueryable<ItSystem>, OperationError> result)
        {
            _systemUsageMigrationService.Setup(x => x.GetUnusedItSystemsByOrganizationQuery(orgId, numberOfItSystems, getPublic, condition)).Returns(result);
        }

        private void ExpectResolveIdReturns<TEntity>(Guid uuid, Maybe<int> result) where TEntity : class, IHasUuid, IHasId
        {
            _identityResolver.Setup(x => x.ResolveDbId<TEntity>(uuid)).Returns(result);
        }
    }
}
