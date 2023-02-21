using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.References;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainServices;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.ItSystems
{
    public class ItSystemWriteServiceTest : WithAutoFixture
    {
        private readonly RightsHolderSystemService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<ITaskRefRepository> _taskRefRepositoryMock;
        private readonly Mock<IGlobalAdminNotificationService> _globalAdminNotificationServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IDatabaseControl> _dbControlMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;

        public ItSystemWriteServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _taskRefRepositoryMock = new Mock<ITaskRefRepository>();
            _globalAdminNotificationServiceMock = new Mock<IGlobalAdminNotificationService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _referenceServiceMock = new Mock<IReferenceService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _dbControlMock = new Mock<IDatabaseControl>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _sut = new RightsHolderSystemService(
                _userContextMock.Object,
                _organizationRepositoryMock.Object,
                _itSystemServiceMock.Object,
                _taskRefRepositoryMock.Object,
                _globalAdminNotificationServiceMock.Object,
                _transactionManagerMock.Object,
                _userRepositoryMock.Object,
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now),
                Mock.Of<ILogger>(),
                _referenceServiceMock.Object,
                _authorizationContextMock.Object,
                _dbControlMock.Object,
                _domainEventsMock.Object);
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            fixture.Register(() => new SystemUpdateParameters
            {
                Name = A<string>().AsChangedValue(),
                Description = A<string>().AsChangedValue(),
                ExternalReferences = new UpdatedExternalReferenceProperties(A<string>(), A<string>(), A<string>(), true).WrapAsEnumerable().FromNullable(),
                FormerName = A<string>().AsChangedValue(),
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue(),
                BusinessTypeUuid = ((Guid?)A<Guid>()).AsChangedValue(),
                TaskRefUuids = Many<Guid>().AsChangedValue()
            });
        }

        [Fact]
        public void CreateNewSystem_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
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

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parentSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectBatchUpdateReferencesReturns(itSystem.Id, inputParameters, Maybe<OperationError>.None);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateTaskRefsReturns(itSystem.Id, expectedTaskRefIds, itSystem);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_GetRightsHolderOrganizationFails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();

            ExpectGetOrganizationReturns(organizationUuid, Maybe<Organization>.None);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_Create_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateRightsHolder_Fails()
        {
            //Arrange
            var inputParameters = new SystemUpdateParameters()
            {
                Name = A<string>().AsChangedValue()
            };
            var organizationUuid = A<Guid>();
            var newRightsHolderUuid = A<Guid>();
            inputParameters.RightsHolderUuid = ((Guid?)newRightsHolderUuid).AsChangedValue();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var newRightsHolderId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectGetOrganizationReturns(newRightsHolderUuid, new Organization { Id = newRightsHolderId, Uuid = newRightsHolderUuid });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, newRightsHolderUuid, operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdatePreviousName_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateDescription_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateMainUrlReference_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectBatchUpdateReferencesReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_GetParentSystemFails()
        {
            //Arrange
            var operationFailures = Enum.GetValues(typeof(OperationFailure)).Cast<OperationFailure>().Where(x => x != OperationFailure.NotFound);
            Configure(x => x.Inject(operationFailures.OrderBy(_ => A<int>()).First()));
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectBatchUpdateReferencesReturns(itSystem.Id, inputParameters, Maybe<OperationError>.None);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.NewValue.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateParentSystemFails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectBatchUpdateReferencesReturns(itSystem.Id, inputParameters, Maybe<OperationError>.None);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parentSystem);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateBusinessTypeFails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<SystemUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectBatchUpdateReferencesReturns(itSystem.Id, inputParameters, Maybe<OperationError>.None);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parentSystem);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_KLE_Duplicates_By_Uuid()
        {
            //Arrange
            var overlapKey = A<Guid>();
            var uniqueKey = A<Guid>();
            var organizationUuid = A<Guid>();
            var inputParameters = new SystemUpdateParameters
            {
                Name = A<string>().AsChangedValue(),
                TaskRefUuids = new[] { overlapKey, uniqueKey, overlapKey }.AsChangedValue<IEnumerable<Guid>>()
            };
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };

            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uniqueKey)).Returns(new TaskRef() { Id = A<int>() });
            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(overlapKey)).Returns(new TaskRef() { Id = A<int>() });

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);

            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Contains("Overlapping KLE. Please specify the same KLE only once. KLE resolved by uuid", result.Error.Message.Value);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_ProvidedTaskUuidIsInvalid()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = new SystemUpdateParameters
            {
                Name = A<string>().AsChangedValue(),
                TaskRefUuids = Many<Guid>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };

            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(It.IsAny<Guid>())).Returns(Maybe<TaskRef>.None);

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgDbId });
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, organizationUuid, itSystem);
            //Act
            var result = _sut.CreateNewSystem(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Can_UpdateItSystem()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<SystemUpdateParameters>();
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
            ExpectAllowModifyReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name.NewValue, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectBatchUpdateReferencesReturns(itSystem.Id, parameters, Maybe<OperationError>.None);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parent);
            ExpectUpdateParentSystemReturns(itSystem.Id, parent, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateTaskRefsReturns(itSystem.Id, expectedTaskRefIds, itSystem);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Cannot_Update_If_KLE_Has_Duplicates()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                TaskRefUuids = Many<Guid>().AsChangedValue(),
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };

            var taskRefs = new Dictionary<string, TaskRef>();
            foreach (var uuid in parameters.TaskRefUuids.NewValue)
            {
                var taskRef = new TaskRef { Id = 1 };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uuid)).Returns(taskRef);
                taskRefs[uuid.ToString()] = taskRef;
            }

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_KLE_Has_Invalid_Ids()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                TaskRefUuids = Many<Guid>().AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            foreach (var taskRefKey in parameters.TaskRefUuids.NewValue)
            {
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(taskRefKey)).Returns(Maybe<TaskRef>.None);
            }

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_Business_Type_Update_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                BusinessTypeUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, parameters, operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_Parent_Update_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), parent);
            ExpectUpdateParentSystemReturns(itSystem.Id, parent, operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_Parent_Resolution_Fails()
        {
            //Arrange
            var operationFailures = Enum.GetValues(typeof(OperationFailure)).Cast<OperationFailure>().Where(x => x != OperationFailure.NotFound);
            Configure(x => x.Inject(operationFailures.OrderBy(_ => A<int>()).First()));
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_Parent_Resolution_Fails_With_NotFound()
        {
            //Arrange
            Configure(x => x.Inject(OperationFailure.NotFound));
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                ParentSystemUuid = ((Guid?)A<Guid>()).AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.NewValue.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.NotSame(operationError, result.Error);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_UpdateReferences_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                ExternalReferences = Many<UpdatedExternalReferenceProperties>().ToList().FromNullable<IEnumerable<UpdatedExternalReferenceProperties>>()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectBatchUpdateReferencesReturns(itSystem.Id, parameters, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_UpdateDescription_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                Description = A<string>().AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(error, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_UpdatePreviousName_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                FormerName = A<string>().AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(error, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_Update_If_UpdateName_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = new SystemUpdateParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name.NewValue, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(error, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        private void ExpectUpdateNameReturns(int id, string name, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateName(id, name)).Returns(result);
        }

        private void ExpectUpdateTaskRefsReturns(int systemId, List<int> expectedTaskRefIds, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateTaskRefs(systemId, It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(expectedTaskRefIds)))).Returns(result);
        }

        private void ExpectUpdateBusinessTypeReturns(int systemId, SharedSystemUpdateParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateBusinessType(systemId, inputParameters.BusinessTypeUuid.NewValue)).Returns(result);
        }

        private void ExpectUpdateParentSystemReturns(int systemId, ItSystem parentSystem, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateParentSystem(systemId, parentSystem.Id)).Returns(result);
        }

        private void ExpectBatchUpdateReferencesReturns(int systemId, SharedSystemUpdateParameters inputParameters, Maybe<OperationError> error)
        {
            _referenceServiceMock.Setup(x => x.UpdateExternalReferences(ReferenceRootType.System, systemId, inputParameters.ExternalReferences.GetValueOrDefault())).Returns(error);
        }

        private void ExpectUpdateDescriptionReturns(int systemId, SharedSystemUpdateParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateDescription(systemId, inputParameters.Description.NewValue)).Returns(result);
        }

        private void ExpectUpdatePreviousNameReturns(int systemId, SharedSystemUpdateParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdatePreviousName(systemId, inputParameters.FormerName.NewValue)).Returns(result);
        }

        private void ExpectUpdateRightsHolderReturns(int systemId, Guid rightsHolderUuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateRightsHolder(systemId, rightsHolderUuid)).Returns(result);
        }


        private void ExpectSystemServiceGetSystemReturns(Guid uuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(uuid)).Returns(result);
        }

        private void ExpectSystemServiceCreateItSystemReturns(int orgDbId, SystemUpdateParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock
                .Setup(x => x.CreateNewSystem(orgDbId, inputParameters.Name.NewValue, null))
                .Returns(result);
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

        private void ExpectAllowModifyReturns(ItSystem itSystem, bool val)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(itSystem)).Returns(true);
        }
    }
}
