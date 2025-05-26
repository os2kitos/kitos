using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations.Handlers;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Handlers
{
    public class ReportPendingFkOrganizationChangesToStakeHoldersHandlerTest : WithAutoFixture
    {
        private readonly ReportPendingFkOrganizationChangesToStakeHoldersHandler _sut;
        private readonly Mock<IStsOrganizationSystemService> _stsOrgUnitServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly DateTime _now;
        private readonly Mock<IDatabaseControl> _databaseControlMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;

        public ReportPendingFkOrganizationChangesToStakeHoldersHandlerTest()
        {
            _stsOrgUnitServiceMock = new Mock<IStsOrganizationSystemService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _now = DateTime.UtcNow;
            _databaseControlMock = new Mock<IDatabaseControl>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _sut = new ReportPendingFkOrganizationChangesToStakeHoldersHandler(
                _stsOrgUnitServiceMock.Object,
                Mock.Of<ILogger>(),
                _transactionManagerMock.Object,
                Mock.Of<IOperationClock>(x => x.Now == _now),
                _databaseControlMock.Object,
                _domainEventsMock.Object);
        }

        [Fact]
        public void Execute_Fails_If_StsConnection_Fails()
        {
            //Arrange
            var command = new ReportPendingFkOrganizationChangesToStakeHolders(new Organization(), new Mock<IExternalOrganizationalHierarchyConnection>().Object);
            var operationError = A<DetailedOperationError<ResolveOrganizationTreeError>>();
            var transactionMock = ExpectTransaction();
            ExpectResolveOrganizationTree(command, operationError);

            //Act
            var error = _sut.Execute(command);

            //Assert
            VerifyFailed(error, operationError, transactionMock);
        }

        [Fact]
        public void Execute_Fails_If_Consequence_Calculation_Fails()
        {
            //Arrange
            var command = new ReportPendingFkOrganizationChangesToStakeHolders(new Organization(), new Mock<IExternalOrganizationalHierarchyConnection>().Object);
            var transactionMock = ExpectTransaction();
            var externalTree = CreateExternalTree();
            ExpectResolveOrganizationTree(command, externalTree);

            //Act
            var error = _sut.Execute(command);

            //Assert badState since the org state is not connected
            var expectedError = new DetailedOperationError<ResolveOrganizationTreeError>(OperationFailure.BadState, ResolveOrganizationTreeError.FailedResolvingUuid);
            VerifyFailed(error, expectedError, transactionMock);
        }

        [Fact]
        public void Execute_Raises_Domain_Event_When_Changes_Exist_And_Updates_Check_Date()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(new OrganizationUnit {Organization = organization,Name = A<string>()});
            var externalTree1 = CreateExternalTree();
            organization.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, externalTree1, Maybe<int>.None, false);
            var command = new ReportPendingFkOrganizationChangesToStakeHolders(organization, new Mock<IExternalOrganizationalHierarchyConnection>().Object);
            var transactionMock = ExpectTransaction();
            var externalTree2 = CreateExternalTree();
            ExpectResolveOrganizationTree(command, externalTree2);

            //Act
            var error = _sut.Execute(command);

            //Assert that an event was raised with a root change
            Assert.True(error.IsNone);
            transactionMock.Verify(x => x.Commit(), Times.Once());
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.Once());
            _domainEventsMock.Verify(x=>x.Raise(It.Is<PendingExternalOrganizationUpdatesResolved>(ev=>ev.Organization == organization && ev.Changes.Entries.Any(entry => entry.Type == ConnectionUpdateOrganizationUnitChangeType.RootChanged))));

        }

        private ExternalOrganizationUnit CreateExternalTree()
        {
            return new ExternalOrganizationUnit(A<Guid>(), A<string>(), new Dictionary<string, string>(), Array.Empty<ExternalOrganizationUnit>());
        }

        private void VerifyFailed(Maybe<OperationError> error, DetailedOperationError<ResolveOrganizationTreeError> expectedError, Mock<IDatabaseTransaction> transactionMock)
        {
            Assert.True(error.HasValue);
            Assert.Equal(expectedError.FailureType, error.Value.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never());
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.Never());
            _domainEventsMock.Verify(x => x.Raise(It.IsAny<PendingExternalOrganizationUpdatesResolved>()), Times.Never());
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private void ExpectResolveOrganizationTree(ReportPendingFkOrganizationChangesToStakeHolders command, Result<ExternalOrganizationUnit,
            DetailedOperationError<ResolveOrganizationTreeError>> result)
        {
            _stsOrgUnitServiceMock.Setup(x => x.ResolveOrganizationTree(command.Organization)).Returns(result);
        }
    }
}
