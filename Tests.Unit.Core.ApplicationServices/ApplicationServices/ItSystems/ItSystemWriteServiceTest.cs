using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.ApplicationServices.System.Write;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.TaskRefs;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.ItSystems
{
    public class ItSystemWriteServiceTest : WithAutoFixture
    {
        private readonly ItSystemWriteService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<ITaskRefRepository> _taskRefRepositoryMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IDatabaseControl> _dbControlMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;

        public ItSystemWriteServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _taskRefRepositoryMock = new Mock<ITaskRefRepository>();
            _referenceServiceMock = new Mock<IReferenceService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _dbControlMock = new Mock<IDatabaseControl>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _sut = new ItSystemWriteService(
                _userContextMock.Object,
                _organizationRepositoryMock.Object,
                _itSystemServiceMock.Object,
                _taskRefRepositoryMock.Object,
                _transactionManagerMock.Object,
                Mock.Of<ILogger>(),
                _referenceServiceMock.Object,
                _authorizationContextMock.Object,
                _dbControlMock.Object,
                _domainEventsMock.Object,
                _identityResolverMock.Object
                );
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
            _domainEventsMock.Verify(x => x.Raise(It.IsAny<EntityUpdatedEventWithSnapshot<ItSystem, ItSystemSnapshot>>()), Times.Once);
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

        [Fact]
        public void Can_Add_ExternalReference()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var properties = CreateExternalReferenceProperties();
            var externalReference = new ExternalReference();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectAddExternalReferenceReturns(itSystem.Id, properties, externalReference);

            //Act
            var result = _sut.AddExternalReference(systemUuid, properties);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(externalReference, result.Value);
        }

        [Fact]
        public void Add_ExternalReference_Returns_Error_When_Creation_Failed()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var properties = CreateExternalReferenceProperties();
            var expectedOperationFailure = A<OperationFailure>();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectAddExternalReferenceReturns(itSystem.Id, properties, new OperationError(expectedOperationFailure));

            //Act
            var result = _sut.AddExternalReference(systemUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedOperationFailure, result.Error.FailureType);
        }

        [Fact]
        public void Add_ExternalReference_Returns_Forbidden_When_User_Has_No_Write_Access()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var properties = CreateExternalReferenceProperties();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.AddExternalReference(systemUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Add_ExternalReference_Returns_Error_When_Get_Usage_Failed()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var properties = CreateExternalReferenceProperties();
            var expectedFailure = A<OperationFailure>();

            ExpectSystemServiceGetSystemReturns(systemUuid, new OperationError(expectedFailure));

            //Act
            var result = _sut.AddExternalReference(systemUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_ExternalReference()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var properties = CreateExternalReferenceProperties();
            var externalReference = new ExternalReference();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectUpdateExternalReferenceReturns(itSystem.Id, referenceUuid, properties, externalReference);

            //Act
            var result = _sut.UpdateExternalReference(systemUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(externalReference, result.Value);
        }

        [Fact]
        public void Update_ExternalReference_Returns_Error_When_Update_Failed()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var properties = CreateExternalReferenceProperties();
            var expectedFailure = A<OperationFailure>();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectUpdateExternalReferenceReturns(itSystem.Id, referenceUuid, properties, new OperationError(expectedFailure));

            //Act
            var result = _sut.UpdateExternalReference(systemUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Update_ExternalReference_Returns_Forbidden_When_User_Has_No_Write_Access()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var properties = CreateExternalReferenceProperties();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.UpdateExternalReference(systemUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Update_ExternalReference_Returns_Error_When_GetUsage_Failed()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var properties = CreateExternalReferenceProperties();
            var expectedFailure = A<OperationFailure>();

            ExpectSystemServiceGetSystemReturns(systemUuid, new OperationError(expectedFailure));

            //Act
            var result = _sut.UpdateExternalReference(systemUuid, referenceUuid, properties);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Can_Delete_ExternalReference()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var externalReference = new ExternalReference();
            var referenceId = A<int>();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ExternalReference>(referenceUuid, referenceId);
            ExpectRemoveExternalReferenceReturns(referenceId, externalReference);

            //Act
            var result = _sut.DeleteExternalReference(systemUuid, referenceUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(externalReference, result.Value);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_Error_When_Failed_To_Delete_Reference()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };
            var referenceId = A<int>();
            var expectedFailure = A<OperationFailure>();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ExternalReference>(referenceUuid, referenceId);
            ExpectRemoveExternalReferenceReturns(referenceId, expectedFailure);

            //Act
            var result = _sut.DeleteExternalReference(systemUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedFailure, result.Error.FailureType);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_NotFound_When_ExternalReferenceId_Was_Not_Found()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ExternalReference>(referenceUuid, Maybe<int>.None);

            //Act
            var result = _sut.DeleteExternalReference(systemUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_Forbidden_When_User_Has_No_Write_Access()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var itSystem = new ItSystem { Id = A<int>() };

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.DeleteExternalReference(systemUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Delete_ExternalReference_Returns_Error_When_Failed_To_Get_Usage()
        {
            //Arrange
            var referenceUuid = A<Guid>();
            var systemUuid = A<Guid>();
            var operationFailure = A<OperationFailure>();

            ExpectSystemServiceGetSystemReturns(systemUuid, new OperationError(operationFailure));

            //Act
            var result = _sut.DeleteExternalReference(systemUuid, referenceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationFailure, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_Legal_Properties()
        {
            var systemUuid = A<Guid>();
            var system = new ItSystem { Uuid = systemUuid };
            var parameters = A<LegalUpdateParameters>();
            ExpectSystemServiceGetSystemReturns(systemUuid, system);
            ExpectHasLegalChangePermissionReturns(true);
            var transaction = ExpectTransactionBegins();

            var result = _sut.LegalPropertiesUpdate(systemUuid, parameters);

            Assert.True(result.Ok);
            Assert.Equal(parameters.SystemName.NewValue, result.Value.LegalName);
            Assert.Equal(parameters.DataProcessorName.NewValue, result.Value.LegalDataProcessorName);
            transaction.Verify(x => x.Commit(), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Only_Update_Legal_Properties_With_Permission(bool hasPermission)
        {
            var systemUuid = A<Guid>();
            var system = new ItSystem { Uuid = systemUuid };
            ExpectSystemServiceGetSystemReturns(systemUuid, system);
            ExpectHasLegalChangePermissionReturns(hasPermission);
            ExpectTransactionBegins();

            var result = _sut.LegalPropertiesUpdate(systemUuid, A<LegalUpdateParameters>());

            Assert.Equal(hasPermission, result.Ok);
        }

        private void ExpectHasLegalChangePermissionReturns(bool hasPermission)
        {
            _authorizationContextMock.Setup(x => x.HasPermission(It.IsAny<ChangeLegalSystemPropertiesPermission>()))
                .Returns(hasPermission);
        }

        private ExternalReferenceProperties CreateExternalReferenceProperties()
        {
            return new ExternalReferenceProperties(A<string>(), A<string>(), A<string>(), A<bool>());
        }

        private void ExpectAddExternalReferenceReturns(int usageId, ExternalReferenceProperties properties, Result<ExternalReference, OperationError> result)
        {
            _referenceServiceMock.Setup(x => x.AddReference(usageId, ReferenceRootType.System, properties)).Returns(result);
        }

        private void ExpectUpdateExternalReferenceReturns(int usageId, Guid externalReferenceUuid, ExternalReferenceProperties properties, Result<ExternalReference, OperationError> result)
        {
            _referenceServiceMock.Setup(x => x.UpdateReference(usageId, ReferenceRootType.System, externalReferenceUuid, properties)).Returns(result);
        }

        private void ExpectRemoveExternalReferenceReturns(int externalReferenceId, Result<ExternalReference, OperationFailure> result)
        {
            _referenceServiceMock.Setup(x => x.DeleteByReferenceId(externalReferenceId)).Returns(result);
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
            _authorizationContextMock.Setup(x => x.AllowModify(itSystem)).Returns(val);
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid referenceUuid, Maybe<int> referenceId)
        {
            _identityResolverMock.Setup(x => x.ResolveDbId<ExternalReference>(referenceUuid)).Returns(referenceId);
        }
    }
}
