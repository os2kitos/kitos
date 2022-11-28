using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Organizations;
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
using Xunit.Abstractions;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class StsOrganizationSynchronizationServiceTest : WithAutoFixture
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IStsOrganizationUnitService> _stsOrganizationUnitService;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly StsOrganizationSynchronizationService _sut;
        private readonly Mock<IDatabaseControl> _dbControlMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IGenericRepository<OrganizationUnit>> _organizationUnitRepositoryMock;
        private readonly ActiveUserIdContext _activeUserIdContext;
        private readonly Mock<IGenericRepository<StsOrganizationChangeLog>> _stsOrganziationChangeLogRepositoryMock;
        private readonly Mock<IOperationClock> _operationClock;

        public StsOrganizationSynchronizationServiceTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _stsOrganizationUnitService = new Mock<IStsOrganizationUnitService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _dbControlMock = new Mock<IDatabaseControl>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _organizationUnitRepositoryMock = new Mock<IGenericRepository<OrganizationUnit>>();
            _activeUserIdContext = new ActiveUserIdContext(A<int>());
            _stsOrganziationChangeLogRepositoryMock = new Mock<IGenericRepository<StsOrganizationChangeLog>>();
            _operationClock = new Mock<IOperationClock>();

            _sut = new StsOrganizationSynchronizationService(
                _authorizationContextMock.Object,
                _stsOrganizationUnitService.Object,
                _organizationServiceMock.Object,
                Mock.Of<ILogger>(),
                Mock.Of<IStsOrganizationService>(),
                _dbControlMock.Object,
                _transactionManagerMock.Object,
                _domainEventsMock.Object,
                _organizationUnitRepositoryMock.Object,
                _activeUserIdContext,
                _stsOrganziationChangeLogRepositoryMock.Object,
                _operationClock.Object);
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            fixture.Inject<IEnumerable<ExternalOrganizationUnit>>(Array.Empty<ExternalOrganizationUnit>()); //prevent endless loop when creating StsOrganizationUnit
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Without_Levels_Limit_Returns_Full_Tree()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string, string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, true);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(root, result.Value); //If root is unmodified, then no filtering happened
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetStsOrganizationalHierarchy_With_Level_Limit_Returns_Filtered_Tree(int levels)
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string, string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, true);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, levels);

            //Assert
            Assert.True(result.Ok);
            Assert.NotSame(root, result.Value);
            Assert.Equal(levels, CountMaxLevels(result.Value));
            Assert.True(Compare(root, result.Value, levels - 1));
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_With_Invalid_Limit_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string, string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, true);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, 0);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Fails_If_Organization_Cannot_Be_Loaded()
        {
            //Arrange
            var organizationId = A<Guid>();
            var operationError = A<OperationError>();
            SetupGetOrganizationReturns(organizationId, operationError);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError, result.Error);
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Fails_If_User_Lacks_Permission()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var children = Many<ExternalOrganizationUnit>();
            var root = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string, string>>(), children);
            SetupGetOrganizationReturns(organizationId, organization);
            SetupResolveOrganizationTreeReturns(organization, root);
            SetupHasPermissionReturns(organization, false);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetStsOrganizationalHierarchy_Fails_If_LoadHierarchy_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            var operationError = A<DetailedOperationError<ResolveOrganizationTreeError>>();
            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, operationError);

            //Act
            var result = _sut.GetStsOrganizationalHierarchy(organizationId, Maybe<int>.None);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Connect_Hierarchy_Returns_Success_And_Imports_External_Units_Into_Kitos(bool onlyRoot, bool subscribe)
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(new OrganizationUnit { Organization = organization });
            var children = Many<ExternalOrganizationUnit>();
            var externalRoot = new ExternalOrganizationUnit(A<Guid>(), A<string>(), A<Dictionary<string, string>>(), children);

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Connect(organizationId, onlyRoot ? 1 : Maybe<int>.None, subscribe);

            //Assert
            Assert.False(error.HasValue);
            var connection = organization.StsOrganizationConnection;
            Assert.True(connection.Connected);
            Assert.Equal(subscribe, connection.SubscribeToUpdates);
            Assert.Equal(onlyRoot ? 1 : (int?)null, connection.SynchronizationDepth);
            VerifyChangesSaved(transaction, organization);

            var kitosOrgRoot = organization.GetRoot();

            //Verify that the root was updated
            //Deep validation of the import logic is handled in OrganizationTest.cs
            Assert.Equal(OrganizationUnitOrigin.STS_Organisation, kitosOrgRoot.Origin); //verify that origin of he root has changed
            Assert.Equal(externalRoot.Uuid, kitosOrgRoot.ExternalOriginUuid); //verify that origin of he root has changed
            Assert.Equal(externalRoot.Name, kitosOrgRoot.Name); //verify that origin of he root has changed
            Assert.Equal(!onlyRoot, kitosOrgRoot.Children.Any()); //If ony root (level 1) was requested validate the expected effect

            //Verify that the logs were added
            var logs = connection.StsOrganizationChangeLogs.ToList();
            var log = Assert.Single(logs);
            foreach (var consequenceLog in log.Entries)
            {
                Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Added, consequenceLog.Type);
            }
        }

        [Fact]
        public void Connect_Hierarchy_Fails_If_Org_Tree_Resolution_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(new OrganizationUnit { Organization = organization });

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, A<DetailedOperationError<ResolveOrganizationTreeError>>());
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Connect(organizationId, Maybe<int>.None, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Null(organization.StsOrganizationConnection);
            VerifyChangesNotSaved(transaction, organization);
        }

        [Fact]
        public void Connect_Hierarchy_Fails_If_HasPermission_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(new OrganizationUnit { Organization = organization });

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, false);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Connect(organizationId, Maybe<int>.None, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
            Assert.Null(organization.StsOrganizationConnection);
            VerifyChangesNotSaved(transaction, organization, false);
        }

        [Fact]
        public void Connect_Hierarchy_Fails_If_GetOrganization_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(new OrganizationUnit { Organization = organization });

            SetupGetOrganizationReturns(organizationId, A<OperationError>());
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Connect(organizationId, Maybe<int>.None, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Null(organization.StsOrganizationConnection);
            VerifyChangesNotSaved(transaction, organization, false);
        }

        [Fact]
        public void UpdateConnection_Performs_Update_Of_Existing_Synchronized_Tree()
        {
            //Arrange
            var organizationId = A<Guid>();
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

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, newDepth, false);

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
        public void UpdateConnection_Fails_If_Not_Already_Connected()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = false,
            };
            var externalRoot = organization.GetRoot().Transform(ToExternalOrganizationUnit);

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, 3, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadState, error.Value.FailureType);
            VerifyChangesNotSaved(transaction, organization);
        }

        [Fact]
        public void UpdateConnection_Fails_If_LoadOrgUnits_Fail()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
            };
            var resolveOrgUnitsError = A<DetailedOperationError<ResolveOrganizationTreeError>>();

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, resolveOrgUnitsError);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, 3, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(resolveOrgUnitsError.FailureType, error.Value.FailureType);
            VerifyChangesNotSaved(transaction, organization);
        }

        [Fact]
        public void UpdateConnection_Fails_If_Auth_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
            };

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, false);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, 3, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
            VerifyChangesNotSaved(transaction, organization, false);
        }

        [Fact]
        public void UpdateConnection_Fails_If_GetOrganization_Fails()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            organization.OrgUnits.Add(CreateOrganizationUnit(organization, true)); //Add the root
            organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Organization = organization,
                Connected = true,
            };
            var getOrganizationError = A<OperationError>();

            SetupGetOrganizationReturns(organizationId, getOrganizationError);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, 3, false);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(getOrganizationError.FailureType, error.Value.FailureType);
            VerifyChangesNotSaved(transaction, organization, false);
        }

        [Fact]
        public void Disconnect_Returns_Fails_If_GetOrganization_Returns_Error()
        {
            //Arrange
            var organizationId = A<Guid>();
            var getOrganizationError = A<OperationError>();
            SetupGetOrganizationReturns(organizationId, getOrganizationError);

            //Act
            var error = _sut.Disconnect(organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(getOrganizationError, error.Value);
        }

        [Fact]
        public void Disconnect_Returns_Fails_UnAuthorized_To_Disconnect()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, false);

            //Act
            var error = _sut.Disconnect(organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void Disconnect_Returns_Fails_Organization_Is_Not_Connected()
        {
            //Arrange
            var organizationId = A<Guid>();
            var organization = new Organization();
            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Disconnect(organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.BadState, error.Value.FailureType);
            transaction.Verify(x => x.Rollback(), Times.Once());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Disconnect_Succeeds_And_Converts_All_Imported_Org_Units_To_Kitos_Units(bool subscribeBeforeDisconnect)
        {
            //Arrange
            var organizationId = A<Guid>();
            var unaffectedUnit = new OrganizationUnit();
            var affectedUnit1 = new OrganizationUnit()
            {
                Parent = unaffectedUnit,
                Origin = OrganizationUnitOrigin.STS_Organisation,
                ExternalOriginUuid = A<Guid>()
            };
            var affectedUnit2 = new OrganizationUnit()
            {
                Parent = unaffectedUnit,
                Origin = OrganizationUnitOrigin.STS_Organisation,
                ExternalOriginUuid = A<Guid>()
            };
            var stsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = true,
                SynchronizationDepth = A<int>(),
                SubscribeToUpdates = subscribeBeforeDisconnect
            };
            var organization = new Organization
            {
                StsOrganizationConnection = stsOrganizationConnection,
                OrgUnits = new List<OrganizationUnit>
                {
                    unaffectedUnit ,
                    affectedUnit1,
                    affectedUnit2
                }
            };
            stsOrganizationConnection.Organization = organization;

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            var transaction = ExpectTransaction();

            //Act
            var error = _sut.Disconnect(organizationId);

            //Assert
            Assert.False(error.HasValue);
            transaction.Verify(x => x.Commit(), Times.Once());
            _dbControlMock.Verify(x => x.SaveChanges(), Times.Once());
            Assert.False(organization.StsOrganizationConnection.Connected);
            Assert.Null(organization.StsOrganizationConnection.SynchronizationDepth);
            Assert.False(organization.StsOrganizationConnection.SubscribeToUpdates);
            foreach (var organizationUnit in organization.OrgUnits)
            {
                Assert.Equal(OrganizationUnitOrigin.Kitos, organizationUnit.Origin);
                Assert.Null(organizationUnit.ExternalOriginUuid);
            }
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<OrganizationUnit>>(u => u.Entity == affectedUnit1)), Times.Once());
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<OrganizationUnit>>(u => u.Entity == affectedUnit2)), Times.Once());
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<OrganizationUnit>>(u => u.Entity == unaffectedUnit)), Times.Never());
        }

        [Fact]
        public void UpdateConnection_Logs_Rename_Changes()
        {
            //Arrange
            var organizationId = A<Guid>();
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

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, newDepth, false);

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Renamed, log.Type);
        }
        
        [Fact]
        public void UpdateConnection_Logs_Addition_Changes()
        {
            //Arrange
            var organizationId = A<Guid>();
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

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, newDepth, false);

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Added, log.Type);
        }

        [Fact]
        public void UpdateConnection_Logs_Deletion_Changes()
        {
            //Arrange
            var organizationId = A<Guid>();
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

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, newDepth, false);

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Deleted, log.Type);
        }

        [Fact]
        public void UpdateConnection_Logs_Conversion_Changes()
        {
            //Arrange
            var organizationId = A<Guid>();
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

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, newDepth, false);

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Converted, log.Type);
        }

        [Fact]
        public void UpdateConnection_Logs_Relocation_Changes()
        {
            //Arrange
            var organizationId = A<Guid>();
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

            SetupGetOrganizationReturns(organizationId, organization);
            SetupHasPermissionReturns(organization, true);
            SetupResolveOrganizationTreeReturns(organization, externalRoot);
            ExpectTransaction();

            //Act
            var error = _sut.UpdateConnection(organizationId, depth, false);

            //Assert
            Assert.False(error.HasValue);

            var changeLog = Assert.Single(organization.StsOrganizationConnection.StsOrganizationChangeLogs);
            var log = Assert.Single(changeLog.Entries);
            Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Moved, log.Type);
        }

        [Fact]
        public void GetChangeLogForOrganization_Returns_Number_Of_Logs()
        {
            var orgUuid = A<Guid>();

            var logs = new List<StsOrganizationConsequenceLog> { new (), new () };
            var changeLogs = new List<StsOrganizationChangeLog> { new () { Entries = logs }, new() { Entries = logs } };

            var stsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = true,
                SynchronizationDepth = A<int>(),
                StsOrganizationChangeLogs = changeLogs
            };
            var organization = new Organization
            {
                Uuid = orgUuid,
                StsOrganizationConnection = stsOrganizationConnection
            };

            SetupGetOrganizationReturns(orgUuid, organization);
            SetupHasPermissionReturns(organization, true);

            var result = _sut.GetChangeLogs(orgUuid, 1);

            Assert.True(result.Ok);
            var logResult = Assert.Single(result.Value);
            ;
            Assert.Equal(2, logResult.GetEntries().Count());
        }

        [Fact]
        public void GetChangeLogForOrganization_Fails_If_GetOrganization_Returns_Error()
        {
            var orgUuid = A<Guid>();
            var getOperationError = A<OperationError>();

            SetupGetOrganizationReturns(orgUuid, getOperationError);

            var result = _sut.GetChangeLogs(orgUuid, 1);

            Assert.True(result.Failed);
            Assert.Equal(getOperationError.FailureType, result.Error);
        }

        [Fact]
        public void GetChangeLogForOrganization_Fails_If_UnAuthorized()
        {
            var orgUuid = A<Guid>();
            var organization = new Organization();

            SetupGetOrganizationReturns(orgUuid, organization);
            SetupHasPermissionReturns(organization, false);

            var result = _sut.GetChangeLogs(orgUuid, 1);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        private void VerifyChangesSaved(Mock<IDatabaseTransaction> transaction, Organization organization)
        {
            _dbControlMock.Verify(x => x.SaveChanges(), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
            transaction.Verify(x => x.Rollback(), Times.Never());
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<Organization>>(org => org.Entity == organization)));
        }

        private void VerifyChangesNotSaved(Mock<IDatabaseTransaction> transaction, Organization organization, bool expectRollback = true)
        {
            _dbControlMock.Verify(x => x.SaveChanges(), Times.Never());
            transaction.Verify(x => x.Commit(), Times.Never());
            transaction.Verify(x => x.Rollback(), expectRollback ? Times.Once() : Times.Never());
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityUpdatedEvent<Organization>>(org => org.Entity == organization)), Times.Never());
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

        private void SetupGetOrganizationReturns(Guid organizationId, Result<Organization, OperationError> organization)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationId, null)).Returns(organization);
        }

        private void SetupHasPermissionReturns(Organization organization, bool value)
        {
            _authorizationContextMock.Setup(x =>
                    x.HasPermission(It.Is<ImportHierarchyFromStsOrganizationPermission>(p => p.Organization == organization)))
                .Returns(value);
        }

        private static int CountMaxLevels(ExternalOrganizationUnit unit)
        {
            const int currentLevelContribution = 1;
            return unit
                .Children
                .Select(CountMaxLevels)
                .OrderByDescending(max => max)
                .FirstOrDefault() + currentLevelContribution;
        }

        private bool Compare(ExternalOrganizationUnit original, ExternalOrganizationUnit filtered, int levelsLeftToCompare)
        {
            Assert.Equal(original.Uuid, filtered.Uuid);
            Assert.Equal(original.Name, filtered.Name);
            Assert.Equal(original.MetaData, filtered.MetaData);
            if (levelsLeftToCompare > 0)
            {
                levelsLeftToCompare--;
                var originalChildren = original.Children.ToList();
                var filteredChildren = filtered.Children.ToList();
                Assert.Equal(originalChildren.Capacity, filteredChildren.Count);
                for (var i = 0; i < originalChildren.Count; i++)
                {
                    var originalChild = originalChildren[i];
                    var filteredChild = filteredChildren[i];
                    if (!Compare(originalChild, filteredChild, levelsLeftToCompare))
                    {
                        _testOutputHelper.WriteLine("Failed during validation of {original} vs {filtered} with {levels} levels left", originalChild, filteredChild, levelsLeftToCompare);
                        return false;
                    }
                }
            }

            return true;
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

        private static ExternalOrganizationUnit ToExternalOrganizationUnit(OrganizationUnit root)
        {
            return new ExternalOrganizationUnit(
                root.ExternalOriginUuid.GetValueOrDefault(),
                root.Name, new Dictionary<string, string>(),
                root.Children.Select(ToExternalOrganizationUnit).ToList()
            );
        }
    }
}
