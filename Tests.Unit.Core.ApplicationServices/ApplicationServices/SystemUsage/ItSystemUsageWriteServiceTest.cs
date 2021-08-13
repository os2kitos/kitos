using System;
using System.Data;
using AutoFixture;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Moq.Language.Flow;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageWriteServiceTest : WithAutoFixture
    {
        private readonly Mock<IItSystemUsageService> _itSystemUsageServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOptionsService<ItSystemUsage, ItSystemCategories>> _systemCatategoriesOptionsServiceMock;
        private readonly Mock<IItContractService> _contractServiceMock;
        private readonly Mock<IItProjectService> _projectServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly ItSystemUsageWriteService _sut;

        public ItSystemUsageWriteServiceTest()
        {
            _itSystemUsageServiceMock = new Mock<IItSystemUsageService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _systemCatategoriesOptionsServiceMock = new Mock<IOptionsService<ItSystemUsage, ItSystemCategories>>();
            _contractServiceMock = new Mock<IItContractService>();
            _projectServiceMock = new Mock<IItProjectService>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _sut = new ItSystemUsageWriteService(_itSystemUsageServiceMock.Object, _transactionManagerMock.Object,
                _itSystemServiceMock.Object, _organizationServiceMock.Object, _authorizationContextMock.Object,
                _systemCatategoriesOptionsServiceMock.Object, _contractServiceMock.Object, _projectServiceMock.Object,
                _domainEventsMock.Object, Mock.Of<ILogger>());
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            var outerFixture = new Fixture();

            //Ensure operation errors are always auto-created WITH both failure and message
            fixture.Register(() => new OperationError(outerFixture.Create<string>(), outerFixture.Create<OperationFailure>()));
        }

        [Fact]
        public void Can_Create_Without_Additional_Data()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var organization = new Organization { Id = A<int>() };
            var itSystem = new ItSystem { Id = A<int>() };
            var itSystemUsage = new ItSystemUsage();

            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetSystemReturns(systemUuid, itSystem);
            ExpectCreateNewReturns(itSystem, organization, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            Assert.True(createResult.Ok);
            Assert.Same(itSystemUsage, createResult.Value);
            AssertTransactionCommitted(transactionMock);
        }

        [Fact]
        public void Create_Fails_With_Usage_CreationFailure()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var organization = new Organization { Id = A<int>() };
            var itSystem = new ItSystem { Id = A<int>() };
            var error = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetSystemReturns(systemUuid, itSystem);
            ExpectCreateNewReturns(itSystem, organization, error);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Same(error, createResult.Error);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Create_Fails_GetOrganization_Failure()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var error = A<OperationError>();

            ExpectGetSystemReturns(systemUuid, error);
            ExpectGetOrganizationReturns(organizationUuid, error);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(error.FailureType, createResult.Error.FailureType);
            Assert.EndsWith(error.Message.Value, createResult.Error.Message.Value);
            AssertTransactionNotCommitted(transactionMock);
        }

        [Fact]
        public void Create_Fails_GetSystem_Failure()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var organizationUuid = A<Guid>();
            var transactionMock = ExpectTransaction();
            var error = A<OperationError>();

            ExpectGetSystemReturns(systemUuid, error);

            //Act
            var createResult = _sut.Create(new SystemUsageCreationParameters(systemUuid, organizationUuid, new SystemUsageUpdateParameters()));

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(error.FailureType, createResult.Error.FailureType);
            Assert.EndsWith(error.Message.Value, createResult.Error.Message.Value);
            AssertTransactionNotCommitted(transactionMock);
        }


        [Fact]
        public void Can_Create_With_General_Data()
        {

        }

        private static void AssertTransactionNotCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        private static void AssertTransactionCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit());
        }

        private void ExpectAllowModifyReturns(ItSystemUsage itSystemUsage, bool value)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(itSystemUsage)).Returns(value);
        }

        private void ExpectCreateNewReturns(ItSystem itSystem, Organization organization, Result<ItSystemUsage, OperationError> result)
        {
            _itSystemUsageServiceMock.Setup(x => x.CreateNew(itSystem.Id, organization.Id)).Returns(result);
        }

        private void ExpectGetSystemReturns(Guid systemUuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(systemUuid)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid organizationUuid, Result<Organization,OperationError> organizationResult)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationUuid, null)).Returns(organizationResult);
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var trasactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(trasactionMock.Object);
            return trasactionMock;
        }

        //TODO: Simulate optional properties as well
    }
}
