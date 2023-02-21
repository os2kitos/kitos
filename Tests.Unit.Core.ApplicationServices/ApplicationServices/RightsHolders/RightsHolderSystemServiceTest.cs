using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.ApplicationServices.System.Write;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.RightsHolders
{
    public class RightsHolderSystemServiceTest : WithAutoFixture
    {
        private readonly RightsHolderSystemService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<ITaskRefRepository> _taskRefRepositoryMock;
        private readonly Mock<IGlobalAdminNotificationService> _globalAdminNotificationServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IDatabaseControl> _dbControlMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IItSystemWriteService> _writeServiceMock;

        public RightsHolderSystemServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _taskRefRepositoryMock = new Mock<ITaskRefRepository>();
            _globalAdminNotificationServiceMock = new Mock<IGlobalAdminNotificationService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _dbControlMock = new Mock<IDatabaseControl>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _writeServiceMock = new Mock<IItSystemWriteService>();
            _sut = new RightsHolderSystemService(
                _userContextMock.Object,
                _organizationRepositoryMock.Object,
                _itSystemServiceMock.Object,
                _globalAdminNotificationServiceMock.Object,
                _transactionManagerMock.Object,
                _userRepositoryMock.Object,
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now),
                Mock.Of<ILogger>(),
                _dbControlMock.Object,
                _domainEventsMock.Object,
                _writeServiceMock.Object);
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            fixture.Register(() => new RightsHolderSystemCreationParameters
            {
                Name = A<string>().AsChangedValue(),
                Description = A<string>().AsChangedValue(),
                FormerName = A<string>().AsChangedValue(),
                ExternalReferences = new UpdatedExternalReferenceProperties(A<string>(), A<string>(), A<string>(), true).WrapAsEnumerable().FromNullable(),
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue(),
                BusinessTypeUuid = ((Guid?)A<Guid>()).AsChangedValue(),
                TaskRefUuids = Many<Guid>().AsChangedValue(),
                RightsHolderProvidedUuid = A<Guid>()
            });
        }

        [Fact]
        public void ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess_Returns_Only_Organizations_Where_User_Has_RightsHolderAccessRole()
        {
            //Arrange
            Organization expectedOrg1 = new() { Id = A<int>() };
            Organization expectedOrg2 = new() { Id = expectedOrg1.Id + 1 };
            Organization noMatchOrg = new() { Id = expectedOrg2.Id + 1 };

            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess))
                .Returns(new[] { expectedOrg1.Id, expectedOrg2.Id });
            _organizationRepositoryMock.Setup(x => x.GetAll())
                .Returns(new EnumerableQuery<Organization>(new[] { noMatchOrg, expectedOrg1, expectedOrg2 }));

            //Act
            var organizations = _sut.ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess().ToList();

            //Assert
            Assert.Equal(2, organizations.Count);
            Assert.Same(expectedOrg1, organizations.First());
            Assert.Same(expectedOrg2, organizations.Last());
        }

        [Fact]
        public void GetAvailableSystems_Returns_Forbidden_If_User_Does_Not_Have_RightsHoldersAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);
            var refinements = new List<IDomainQuery<ItSystem>>();

            //Act
            var result = _sut.GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(refinements);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetAvailableSystems_If_User_Has_RightsHoldersAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(true);
            var expectedResponse = Mock.Of<IQueryable<ItSystem>>();
            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(Many<int>());
            _itSystemServiceMock.Setup(x => x.GetAvailableSystems(It.IsAny<IDomainQuery<ItSystem>[]>())).Returns(expectedResponse);
            var refinements = new List<IDomainQuery<ItSystem>>();

            //Act
            var result = _sut.GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(refinements);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedResponse, result.Value);
        }

        [Fact]
        public void Can_GetAvailableSystems_Applies_Refinements()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(true);
            var expectedSystem = new ItSystem();
            var expectedResponse = new List<ItSystem>() { expectedSystem };
            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(Many<int>());
            _itSystemServiceMock.Setup(x => x.GetAvailableSystems(It.IsAny<IDomainQuery<ItSystem>[]>())).Returns(expectedResponse.AsQueryable());

            var domainQuery = new Mock<IDomainQuery<ItSystem>>();
            domainQuery.Setup(x => x.Apply(It.IsAny<IQueryable<ItSystem>>())).Returns(new List<ItSystem>() { expectedSystem }.AsQueryable());

            var refinements = new List<IDomainQuery<ItSystem>>();

            refinements.Add(domainQuery.Object);

            //Act
            var result = _sut.GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(refinements);

            //Assert
            Assert.True(result.Ok);
            var resultSystem = Assert.Single(result.Value);
            Assert.Same(expectedSystem, resultSystem);
        }

        [Fact]
        public void GetSystemAsRightsHolder_Returns_Forbidden_If_User_Does_Not_Have_RightsHoldersAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);

            //Act
            var result = _sut.GetSystemAsRightsHolder(A<Guid>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetSystemAsRightsHolder_Returns_Forbidden_If_User_Does_Not_Have_RightsHoldersAccess_To_The_Retrieved_System()
        {
            //Arrange
            var systemUuid = A<Guid>();
            ExpectUserHasRightsHolderAccessReturns(true);
            var itSystem = new ItSystem()
            {
                BelongsToId = A<int>()
            };
            _itSystemServiceMock.Setup(x => x.GetSystem(systemUuid)).Returns(itSystem);
            ExpectHasSpecificAccessReturns(itSystem, false);

            //Act
            var result = _sut.GetSystemAsRightsHolder(systemUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetSystemAsRightsHolderIf_User_Has_RightsHoldersAccess()
        {
            //Arrange
            var systemUuid = A<Guid>();
            ExpectUserHasRightsHolderAccessReturns(true);
            var itSystem = new ItSystem()
            {
                BelongsToId = A<int>()
            };
            ExpectHasSpecificAccessReturns(itSystem, true);
            _itSystemServiceMock.Setup(x => x.GetSystem(systemUuid)).Returns(itSystem);

            //Act
            var result = _sut.GetSystemAsRightsHolder(systemUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystem, result.Value);
        }

        [Fact]
        public void CreateNewSystemAsRightsHolder_Returns_Ok()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };

            var taskRefs = new Dictionary<string, TaskRef>();
            foreach (var uuid in inputParameters.TaskRefUuids.NewValue)
            {
                var taskRef = new TaskRef { Id = A<int>() };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uuid)).Returns(taskRef);
                taskRefs[uuid.ToString()] = taskRef;
            }

            var expectedTaskRefIds = taskRefs.Select(tr => tr.Value.Id).ToList();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);

            //Act
            var result = _sut.CreateNewSystemAsRightsHolder(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void CreateNewSystemAsRightsHolder_Returns_Error_If_GetRightsHolderOrganizationFails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();

            ExpectGetOrganizationReturns(rightsHolderUuid, Maybe<Organization>.None);

            //Act
            var result = _sut.CreateNewSystemAsRightsHolder(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystemAsRightsHolder_Returns_Error_If_No_RightsHolderAccess_In_Target_Organization()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, false);

            //Act
            var result = _sut.CreateNewSystemAsRightsHolder(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystemAsRightsHolder_Returns_Error_If_Create_Fails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystemAsRightsHolder(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystemAsRightsHolder_Returns_Error_If_UpdateRightsHolder_Fails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);

            //Act
            var result = _sut.CreateNewSystemAsRightsHolder(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystemAsRightsHolder_Returns_Error_If_GetParentSystemFails()
        {
            //Arrange
            var operationFailures = Enum.GetValues(typeof(OperationFailure)).Cast<OperationFailure>().Where(x => x != OperationFailure.NotFound);
            Configure(x => x.Inject(operationFailures.OrderBy(_ => A<int>()).First()));
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.NewValue.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.CreateNewSystemAsRightsHolder(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystemAsRightsHolder_Returns_Error_If_UserIsNotRightsHolderOfParentSystem()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, false);

            //Act
            var result = _sut.CreateNewSystemAsRightsHolder(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Can_UpdateItSystem()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var taskRefs = new Dictionary<string, TaskRef>();
            foreach (var uuid in parameters.TaskRefUuids.NewValue)
            {
                var taskRef = new TaskRef { Id = A<int>() };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uuid)).Returns(taskRef);
                taskRefs[uuid.ToString()] = taskRef;
            }
            var expectedTaskRefIds = taskRefs.Select(tr => tr.Value.Id).ToList();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, true);

            //Act
            var result = _sut.UpdateAsRightsHolder(systemUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Cannot_UpdateAsRightsHolder_If_No_RightsHolder_Access_To_Parent()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new RightsHolderSystemUpdateParameters()
            {
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, false);

            //Act
            var result = _sut.UpdateAsRightsHolder(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateAsRightsHolder_If_Parent_Resolution_Fails()
        {
            //Arrange
            var operationFailures = Enum.GetValues(typeof(OperationFailure)).Cast<OperationFailure>().Where(x => x != OperationFailure.NotFound);
            Configure(x => x.Inject(operationFailures.OrderBy(_ => A<int>()).First()));
            var systemUuid = A<Guid>();
            var parameters = new RightsHolderSystemUpdateParameters
            {
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.UpdateAsRightsHolder(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateAsRightsHolder_If_Parent_Resolution_Fails_With_NotFound()
        {
            //Arrange
            Configure(x => x.Inject(OperationFailure.NotFound));
            var systemUuid = A<Guid>();
            var parameters = new RightsHolderSystemUpdateParameters
            {
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.UpdateAsRightsHolder(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.NotSame(operationError, result.Error);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateAsRightsHolder_If_System_Is_Deactivated()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>(), Disabled = true };

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);

            //Act
            var result = _sut.UpdateAsRightsHolder(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Deactivate_Deactivates_System_And_Notifies_Global_Admins()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var itSystem = new ItSystem() { Id = A<int>(), BelongsToId = A<int>() };
            var userId = A<int>();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            _userContextMock.Setup(x => x.UserId).Returns(userId);
            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(new User { Email = A<string>() });

            //Act
            var result = _sut.DeactivateAsRightsHolder(systemUuid, reason);

            //Assert
            Assert.True(result.Ok);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Deactivate_Does_Not_Notify_Global_Admin_If_Deactivation_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var itSystem = new ItSystem() { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);

            //Act
            var result = _sut.DeactivateAsRightsHolder(systemUuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Deactivate_Does_Not_Notify_Global_Admin_If_Missing_Access()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var itSystem = new ItSystem() { Id = A<int>(), BelongsToId = A<int>() };

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, false);

            //Act
            var result = _sut.DeactivateAsRightsHolder(systemUuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Deactivate_Does_Not_Notify_Global_Admin_If_Get_System_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var operationError = A<OperationError>();

            ExpectSystemServiceGetSystemReturns(systemUuid, operationError);

            //Act
            var result = _sut.DeactivateAsRightsHolder(systemUuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
        }


        private void ExpectSystemServiceGetSystemReturns(Guid uuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(uuid)).Returns(result);
        }

        private void ExpectSystemServiceCreateItSystemReturns(int orgDbId, RightsHolderSystemCreationParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock
                .Setup(x => x.CreateNewSystem(orgDbId, inputParameters.Name.NewValue, inputParameters.RightsHolderProvidedUuid))
                .Returns(result);
        }

        private void ExpectUserHasRightsHolderAccessInOrganizationReturns(int orgId, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(orgId, OrganizationRole.RightsHolderAccess)).Returns(value);
        }

        private void ExpectGetOrganizationReturns(Guid rightsHolderUuid, Maybe<Organization> organization)
        {
            _organizationRepositoryMock.Setup(x => x.GetByUuid(rightsHolderUuid)).Returns(organization);
        }

        private Mock<IDatabaseTransaction> ExpectTransactionBegins()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private void ExpectHasSpecificAccessReturns(ItSystem itSystem, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(itSystem.BelongsToId.Value, OrganizationRole.RightsHolderAccess))
                .Returns(value);
        }

        private void ExpectUserHasRightsHolderAccessReturns(bool value)
        {
            _userContextMock.Setup(x => x.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)).Returns(value);
        }
    }
}
