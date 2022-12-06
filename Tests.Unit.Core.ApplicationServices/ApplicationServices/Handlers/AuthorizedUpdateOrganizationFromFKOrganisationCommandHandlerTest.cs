using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations.Handlers;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Context;
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
    public class AuthorizedUpdateOrganizationFromFKOrganisationCommandHandlerTest : WithAutoFixture
    {
        private DateTime _now;
        private AuthorizedUpdateOrganizationFromFKOrganisationCommandHandler _sut;
        private Mock<IStsOrganizationUnitService> _stsOrganizationUnitService;
        private Mock<IGenericRepository<OrganizationUnit>> _organizationUnitRepositoryMock;
        private Mock<IDomainEvents> _domainEventsMock;
        private Mock<IDatabaseControl> _databaseControlMock;
        private Mock<ITransactionManager> _transactionManagerMock;
        private Mock<IGenericRepository<StsOrganizationChangeLog>> _stsChangeLogRepositoryMock;

        public AuthorizedUpdateOrganizationFromFKOrganisationCommandHandlerTest()
        {
            CreateSut(Maybe<ActiveUserIdContext>.None);
        }

        [Fact]
        public void Execute_Performs_Update_Of_Existing_Synchronized_Tree()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            const int oldDepth = 2;
            const int newDepth = 3;
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
                SynchronizationDepth = oldDepth
            };
            //Add some children to the root
            organization.AddOrganizationUnit(CreateOrganizationUnit(organization, true), organization.GetRoot());
            organization.AddOrganizationUnit(CreateOrganizationUnit(organization, true), organization.GetRoot());

            // In the external tree, ensure that the last leaf is missing - this should track a deleted unit. 
            // Extensive testing of the import algorithm is not part of this test but part of StsOrganizationalHierarchyUpdateStrategyTest.cs
            // We just need to see that the app service deletes deleted units from db
            var externalRoot = organization.GetRoot().Transform(ToExternalOrganizationUnit);

            //add the leaf that will be removed because it is missing in the external tree
            var expectedDeletion = CreateOrganizationUnit(organization, true);
            organization.AddOrganizationUnit(expectedDeletion, organization.GetRoot());

            //Track a rename on the root and check that an event is raised
            organization.GetRoot().UpdateName(A<string>());

            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, newDepth, false,Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.False(error.HasValue);
            Assert.NotNull(organization.StsOrganizationConnection);
            Assert.True(organization.StsOrganizationConnection.Connected);
            Assert.Equal(newDepth, organization.StsOrganizationConnection.SynchronizationDepth);
            VerifyChangesSaved(transaction, organization);

            _organizationUnitRepositoryMock.Verify(x => x.RemoveRange(It.Is<IEnumerable<OrganizationUnit>>(units => units.Single() == expectedDeletion)), Times.Once());
            Assert.Equal(2, organization.GetRoot().Children.Count);
            Assert.DoesNotContain(expectedDeletion, organization.GetRoot().Children);
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<OrganizationUnit>>(ev => ev.Entity == organization.GetRoot())), Times.Once());
        }

        [Fact]
        public void Execute_Fails_If_Not_Already_Connected()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = false,
            };
            var externalRoot = organization.GetRoot().Transform(ToExternalOrganizationUnit);

            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, 3, false, Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadState, error.Value.FailureType);
            VerifyChangesNotSaved(transaction, organization);
        }

        [Fact]
        public void Execute_Fails_If_LoadOrgUnits_Fail()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
            };
            var resolveOrgUnitsError = A<DetailedOperationError<ResolveOrganizationTreeError>>();

            SetupResolveOrganizationTreeReturns(organization, resolveOrgUnitsError);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, 3, false, Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(resolveOrgUnitsError.FailureType, error.Value.FailureType);
            VerifyChangesNotSaved(transaction, organization,false);
        }

        [Fact]
        public void Execute_Logs_Rename_Changes()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            const int oldDepth = 2;
            const int newDepth = 3;
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
                SynchronizationDepth = oldDepth
            };

            var externalRoot = organization.GetRoot().Transform(ToExternalOrganizationUnit);

            //Track a rename on the root and check that an event is raised
            organization.GetRoot().UpdateName(A<string>());

            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, newDepth, false, Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Renamed, log.Type);
        }

        [Fact]
        public void Execute_Logs_Addition_Changes()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            const int oldDepth = 2;
            const int newDepth = 3;
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
                SynchronizationDepth = oldDepth
            };

            organization.AddOrganizationUnit(CreateOrganizationUnit(organization), organization.GetRoot());

            var externalRoot = organization.GetRoot().Transform(ToExternalOrganizationUnit);

            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, newDepth, false, Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Added, log.Type);
        }

        [Fact]
        public void Execute_Logs_Deletion_Changes()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            const int oldDepth = 2;
            const int newDepth = 1;
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
                SynchronizationDepth = oldDepth
            };

            organization.AddOrganizationUnit(CreateOrganizationUnit(organization, true), organization.GetRoot());

            var externalRoot = organization.GetRoot().Transform(ToExternalOrganizationUnit);

            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, newDepth, false, Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Deleted, log.Type);
        }

        [Fact]
        public void Execute_Logs_Conversion_Changes()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            const int oldDepth = 2;
            const int newDepth = 1;
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
                SynchronizationDepth = oldDepth
            };

            var unitToConvert = CreateOrganizationUnit(organization, true);
            unitToConvert.ResponsibleForItContracts = new List<ItContract> { new() };
            organization.AddOrganizationUnit(unitToConvert, organization.GetRoot());

            var externalRoot = organization.GetRoot().Transform(ToExternalOrganizationUnit);

            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, newDepth, false, Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Converted, log.Type);
        }
        //TODO: Test with preloaded org
        [Fact]
        public void Execute_Logs_Relocation_Changes()
        {
            //Arrange
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            const int depth = 3;
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
                SynchronizationDepth = depth
            };
            var root = organization.GetRoot();
            var child = CreateOrganizationUnit(organization, true);
            organization.AddOrganizationUnit(child, root);
            var child2 = CreateOrganizationUnit(organization, true);
            organization.AddOrganizationUnit(child2, root);

            var externalRoot = root.Transform(ToExternalOrganizationUnit);

            child.AddChild(child2);
            root.Children.Remove(child2);

            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.Execute(new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, depth, false, Maybe<ExternalOrganizationUnit>.None));

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Moved, log.Type);
        }

        private static ExternalOrganizationUnit ToExternalOrganizationUnit(OrganizationUnit root)
        {
            return new ExternalOrganizationUnit(
                root.ExternalOriginUuid.GetValueOrDefault(),
                root.Name, new Dictionary<string, string>(),
                root.Children.Select(ToExternalOrganizationUnit).ToList()
            );
        }

        private void VerifyChangesSaved(Mock<IDatabaseTransaction> transaction, Organization organization)
        {
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
            transaction.Verify(x => x.Rollback(), Times.Never());
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<Organization>>(org => org.Entity == organization)));
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            return transaction;
        }

        private void SetupResolveOrganizationTreeReturns(Organization organization, Result<ExternalOrganizationUnit, DetailedOperationError<ResolveOrganizationTreeError>> root)
        {
            _stsOrganizationUnitService.Setup(x => x.ResolveOrganizationTree(organization)).Returns(root);
        }

        private void VerifyChangesNotSaved(Mock<IDatabaseTransaction> transaction, Organization organization, bool expectRollback = true)
        {
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.Never());
            transaction.Verify(x => x.Commit(), Times.Never());
            transaction.Verify(x => x.Rollback(), expectRollback ? Times.Once() : Times.Never());
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<Organization>>(org => org.Entity == organization)), Times.Never());
        }

        private void CreateSut(Maybe<ActiveUserIdContext> activeUserId)
        {
            _stsOrganizationUnitService = new Mock<IStsOrganizationUnitService>();
            _organizationUnitRepositoryMock = new Mock<IGenericRepository<OrganizationUnit>>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _stsChangeLogRepositoryMock = new Mock<IGenericRepository<StsOrganizationChangeLog>>();
            _sut = new AuthorizedUpdateOrganizationFromFKOrganisationCommandHandler(
                _stsOrganizationUnitService.Object,
                _organizationUnitRepositoryMock.Object,
                Mock.Of<ILogger>(),
                _domainEventsMock.Object,
                _databaseControlMock.Object,
                _transactionManagerMock.Object,
                activeUserId,
                Mock.Of<IOperationClock>(x => x.Now == _now),
                _stsChangeLogRepositoryMock.Object
            );
        }

        private OrganizationUnit CreateOrganizationUnit(Organization organization, bool isExternal = false)
        {
            return new OrganizationUnit
            {
                Organization = organization,
                Id = A<int>(),
                Uuid = A<Guid>(),
                Name = A<string>(),
                ExternalOriginUuid = isExternal ? A<Guid>() : null,
                Origin = isExternal ? OrganizationUnitOrigin.STS_Organisation : OrganizationUnitOrigin.Kitos
            };
        }
    }
}
