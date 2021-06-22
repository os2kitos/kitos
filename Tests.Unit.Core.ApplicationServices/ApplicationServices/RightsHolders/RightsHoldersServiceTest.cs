using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.RightsHolders
{
    public class RightsHoldersServiceTest : WithAutoFixture
    {
        private readonly RightsHoldersService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<ITaskRefRepository> _taskRefRepositoryMock;
        private readonly Mock<IGlobalAdminNotificationService> _globalAdminNotificationServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public RightsHoldersServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _taskRefRepositoryMock = new Mock<ITaskRefRepository>();
            _globalAdminNotificationServiceMock = new Mock<IGlobalAdminNotificationService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _sut = new RightsHoldersService(
                _userContextMock.Object,
                _organizationRepositoryMock.Object,
                _itSystemServiceMock.Object,
                _taskRefRepositoryMock.Object,
                _globalAdminNotificationServiceMock.Object,
                _transactionManagerMock.Object,
                _userRepositoryMock.Object,
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now),
                Mock.Of<ILogger>());
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

            //Act
            var result = _sut.GetSystemsWhereAuthenticatedUserHasRightsHolderAccess();

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
            _itSystemServiceMock.Setup(x => x.GetAvailableSystems(It.IsAny<IDomainQuery<ItSystem>>())).Returns(expectedResponse);

            //Act
            var result = _sut.GetSystemsWhereAuthenticatedUserHasRightsHolderAccess();

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedResponse, result.Value);
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
        public void CreateNewSystem_Returns_Ok()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };

            var taskRefs = new Dictionary<string, TaskRef>();
            foreach (var taskRefKey in inputParameters.TaskRefKeys)
            {
                var taskRef = new TaskRef { Id = A<int>() };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(taskRefKey)).Returns(taskRef);
                taskRefs[taskRefKey] = taskRef;
            }
            foreach (var uuid in inputParameters.TaskRefUuids)
            {
                var taskRef = new TaskRef { Id = A<int>() };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uuid)).Returns(taskRef);
                taskRefs[uuid.ToString()] = taskRef;
            }

            var expectedTaskRefIds = taskRefs.Select(tr => tr.Value.Id).ToList();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateTaskRefsReturns(itSystem.Id, expectedTaskRefIds, itSystem);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_GetRightsHolderOrganizationFails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();

            ExpectGetOrganizationReturns(rightsHolderUuid, Maybe<Organization>.None);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_No_RightsHolderAccess_In_Target_Organization()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, false);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_Create_Fails()
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
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateRightsHolder_Fails()
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
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, operationError);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdatePreviousName_Fails()
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
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateDescription_Fails()
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
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateMainUrlReference_Fails()
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
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_GetParentSystemFails()
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
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UserIsNotRightsHolderOfParentSystem()
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
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, false);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateParentSystemFails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, operationError);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_UpdateBusinessTypeFails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_KLE_Duplicates_By_Key()
        {
            //Arrange
            var overlapKey = A<string>();
            var uniqueKey = A<string>();
            Configure(fixture => fixture.Inject<IEnumerable<Guid>>(new Guid[0]));
            Configure(fixture => fixture.Inject<IEnumerable<string>>(new[] { overlapKey, uniqueKey, overlapKey }));
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };

            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uniqueKey)).Returns(new TaskRef() { Id = A<int>() });
            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(overlapKey)).Returns(new TaskRef() { Id = A<int>() });

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, itSystem);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Contains("Overlapping KLE. Please specify the same KLE only once. KLE resolved by key ", result.Error.Message.Value);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_KLE_Duplicates_By_Uuid()
        {
            //Arrange
            var overlapKey = A<Guid>();
            var uniqueKey = A<Guid>();
            Configure(fixture => fixture.Inject<IEnumerable<Guid>>(new Guid[] { overlapKey, uniqueKey, overlapKey }));
            Configure(fixture => fixture.Inject<IEnumerable<string>>(new string[0]));
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };

            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uniqueKey)).Returns(new TaskRef() { Id = A<int>() });
            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(overlapKey)).Returns(new TaskRef() { Id = A<int>() });

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, itSystem);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Contains("Overlapping KLE. Please specify the same KLE only once. KLE resolved by uuid", result.Error.Message.Value);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_ProvidedTaskKeyIsInvalid()
        {
            //Arrange
            Configure(fixture => fixture.Inject<IEnumerable<Guid>>(new Guid[0]));
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };

            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(It.IsAny<string>())).Returns(Maybe<TaskRef>.None);

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, itSystem);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_ProvidedTaskUuidIsInvalid()
        {
            //Arrange
            Configure(fixture => fixture.Inject<IEnumerable<string>>(new string[0]));
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderSystemCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var itSystem = new ItSystem { Id = A<int>() };
            var parentSystem = new ItSystem() { Id = A<int>(), OrganizationId = A<int>(), BelongsToId = A<int>() };

            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(It.IsAny<Guid>())).Returns(Maybe<TaskRef>.None);

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgDbId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgDbId, true);
            ExpectSystemServiceCreateItSystemReturns(orgDbId, inputParameters, itSystem);
            ExpectSystemServiceGetSystemReturns(inputParameters.ParentSystemUuid.GetValueOrDefault(), parentSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(parentSystem.BelongsToId.Value, true);
            ExpectUpdateRightsHolderReturns(itSystem.Id, rightsHolderUuid, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, inputParameters, itSystem);
            ExpectUpdateParentSystemReturns(itSystem.Id, parentSystem, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, inputParameters, itSystem);

            //Act
            var result = _sut.CreateNewSystem(rightsHolderUuid, inputParameters);

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
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var taskRefs = new Dictionary<string, TaskRef>();
            foreach (var taskRefKey in parameters.TaskRefKeys)
            {
                var taskRef = new TaskRef { Id = A<int>() };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(taskRefKey)).Returns(taskRef);
                taskRefs[taskRefKey] = taskRef;
            }
            foreach (var uuid in parameters.TaskRefUuids)
            {
                var taskRef = new TaskRef { Id = A<int>() };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uuid)).Returns(taskRef);
                taskRefs[uuid.ToString()] = taskRef;
            }
            var expectedTaskRefIds = taskRefs.Select(tr => tr.Value.Id).ToList();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, true);
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
        public void Cannot_UpdateItSystem_If_KLE_Has_Duplicates()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var taskRefs = new Dictionary<string, TaskRef>();
            foreach (var taskRefKey in parameters.TaskRefKeys)
            {
                var taskRef = new TaskRef { Id = 1 };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(taskRefKey)).Returns(taskRef);
                taskRefs[taskRefKey] = taskRef;
            }
            foreach (var uuid in parameters.TaskRefUuids)
            {
                var taskRef = new TaskRef { Id = 1 };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(uuid)).Returns(taskRef);
                taskRefs[uuid.ToString()] = taskRef;
            }

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, true);
            ExpectUpdateParentSystemReturns(itSystem.Id, parent, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, parameters, itSystem);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_KLE_Has_Invalid_Ids()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var taskRefs = new Dictionary<string, TaskRef>();
            foreach (var taskRefKey in parameters.TaskRefKeys)
            {
                var taskRef = new TaskRef { Id = 1 };
                _taskRefRepositoryMock.Setup(x => x.GetTaskRef(taskRefKey)).Returns(Maybe<TaskRef>.None);
                taskRefs[taskRefKey] = taskRef;
            }

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, true);
            ExpectUpdateParentSystemReturns(itSystem.Id, parent, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, parameters, itSystem);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_Business_Type_Update_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, true);
            ExpectUpdateParentSystemReturns(itSystem.Id, parent, itSystem);
            ExpectUpdateBusinessTypeReturns(itSystem.Id, parameters, operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_Parent_Update_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, true);
            ExpectUpdateParentSystemReturns(itSystem.Id, parent, operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_No_RightsHolder_Access_To_Parent()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var parent = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), parent);
            ExpectHasSpecificAccessReturns(parent, false);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_Parent_Resolution_Fails()
        {
            //Arrange
            var operationFailures = Enum.GetValues(typeof(OperationFailure)).Cast<OperationFailure>().Where(x => x != OperationFailure.NotFound);
            Configure(x => x.Inject(operationFailures.OrderBy(_ => A<int>()).First()));
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_Parent_Resolution_Fails_With_NotFound()
        {
            //Arrange
            Configure(x => x.Inject(OperationFailure.NotFound));
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var operationError = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, itSystem);
            ExpectSystemServiceGetSystemReturns(parameters.ParentSystemUuid.GetValueOrDefault(), operationError);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.NotSame(operationError, result.Error);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_UpdateUrlReference_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateMainUrlReferenceReturns(itSystem.Id, parameters, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(error, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_UpdateDescription_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, itSystem);
            ExpectUpdateDescriptionReturns(itSystem.Id, parameters, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(error, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_UpdatePreviousName_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, itSystem);
            ExpectUpdatePreviousNameReturns(itSystem.Id, parameters, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(error, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_UpdateName_Fails()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>() };
            var error = A<OperationError>();

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectUpdateNameReturns(itSystem.Id, parameters.Name, error);

            //Act
            var result = _sut.Update(systemUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(error, result.Error);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Cannot_UpdateItSystem_If_System_Is_Deactivated()
        {
            //Arrange
            var systemUuid = A<Guid>();
            var parameters = A<RightsHolderSystemUpdateParameters>();
            var itSystem = new ItSystem { Id = A<int>(), BelongsToId = A<int>(), Disabled = true };

            var transaction = ExpectTransactionBegins();
            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);

            //Act
            var result = _sut.Update(systemUuid, parameters);

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
            var itSystem = new ItSystem() { Id = A<int>(), BelongsToId = A<int>()};
            var userId = A<int>();

            ExpectSystemServiceGetSystemReturns(systemUuid, itSystem);
            ExpectHasSpecificAccessReturns(itSystem, true);
            ExpectDeactivateReturns(itSystem.Id, itSystem);
            _userContextMock.Setup(x => x.UserId).Returns(userId);
            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(new User {Email = A<string>()});

            //Act
            var result = _sut.Deactivate(systemUuid, reason);

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
            ExpectDeactivateReturns(itSystem.Id, operationError);

            //Act
            var result = _sut.Deactivate(systemUuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError,result.Error);
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
            var result = _sut.Deactivate(systemUuid, reason);

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
            var result = _sut.Deactivate(systemUuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        private void ExpectDeactivateReturns(int systemId, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.Deactivate(systemId)).Returns(result);
        }

        private void ExpectUpdateNameReturns(int id, string name, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateName(id, name)).Returns(result);
        }

        private void ExpectUpdateTaskRefsReturns(int systemId, List<int> expectedTaskRefIds, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateTaskRefs(systemId, It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(expectedTaskRefIds)))).Returns(result);
        }

        private void ExpectUpdateBusinessTypeReturns(int systemId, IRightsHolderWritableSystemProperties inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateBusinessType(systemId, inputParameters.BusinessTypeUuid)).Returns(result);
        }

        private void ExpectUpdateParentSystemReturns(int systemId, ItSystem parentSystem, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateParentSystem(systemId, parentSystem.Id)).Returns(result);
        }

        private void ExpectUpdateMainUrlReferenceReturns(int systemId, IRightsHolderWritableSystemProperties inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateMainUrlReference(systemId, inputParameters.UrlReference)).Returns(result);
        }

        private void ExpectUpdateDescriptionReturns(int systemId, IRightsHolderWritableSystemProperties inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateDescription(systemId, inputParameters.Description)).Returns(result);
        }

        private void ExpectUpdatePreviousNameReturns(int systemId, IRightsHolderWritableSystemProperties inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdatePreviousName(systemId, inputParameters.FormerName)).Returns(result);
        }

        private void ExpectUpdateRightsHolderReturns(int systemId, Guid rightsHolderUuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateRightsHolder(systemId, rightsHolderUuid)).Returns(result);
        }


        private void ExpectSystemServiceGetSystemReturns(Guid uuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(uuid)).Returns(result);
        }

        private void ExpectSystemServiceCreateItSystemReturns(int orgDbId, RightsHolderSystemCreationParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock
                .Setup(x => x.CreateNewSystem(orgDbId, inputParameters.Name, inputParameters.RightsHolderProvidedUuid))
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
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transactionMock.Object);
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
