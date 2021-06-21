using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.TaskRefs;
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
        private readonly Mock<IItInterfaceService> _interfaceService;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<ITaskRefRepository> _taskRefRepositoryMock;

        public RightsHoldersServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _interfaceService = new Mock<IItInterfaceService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _taskRefRepositoryMock = new Mock<ITaskRefRepository>();
            _sut = new RightsHoldersService(
                _userContextMock.Object,
                _organizationRepositoryMock.Object,
                _interfaceService.Object,
                _itSystemServiceMock.Object,
                _taskRefRepositoryMock.Object,
                _transactionManagerMock.Object,
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
        public void GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess_Returns_Forbidden_If_User_Does_Not_Have_RightsHolderAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);

            //Act
            var result = _sut.GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess();

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess_If_User_Has_RightsHolderAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(true);
            var expectedResponse = Mock.Of<IQueryable<ItInterface>>();
            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(Many<int>());
            _interfaceService.Setup(x => x.GetAvailableInterfaces(It.IsAny<IDomainQuery<ItInterface>>())).Returns(expectedResponse);

            //Act
            var result = _sut.GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess();

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedResponse, result.Value);
        }

        [Fact]
        public void GetInterfaceAsRightsHolder_Returns_Forbidden_If_User_Does_Not_Have_RightsHolderAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);

            //Act
            var result = _sut.GetInterfaceAsRightsHolder(A<Guid>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetInterfaceAsRightsHolder_Returns_Forbidden_If_User_Does_Not_Have_RightsHolderAccess_To_The_Retrieved_System()
        {
            //Arrange
            var itInterfaceUuid = A<Guid>();
            ExpectUserHasRightsHolderAccessReturns(true);
            var itInterface = new ItInterface()
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = A<int>()
                    }
                }
            };
            _interfaceService.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(itInterface);
            ExpectHasSpecificAccessReturns(itInterface, false);

            //Act
            var result = _sut.GetInterfaceAsRightsHolder(itInterfaceUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetInterfaceAsRightsHolder_If_User_Has_RightsHolderAccess()
        {
            //Arrange
            var itInterfaceUuid = A<Guid>();
            ExpectUserHasRightsHolderAccessReturns(true);
            var itInterface = new ItInterface()
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = A<int>()
                    }
                }
            };
            ExpectHasSpecificAccessReturns(itInterface, true);
            _interfaceService.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(itInterface);

            //Act
            var result = _sut.GetInterfaceAsRightsHolder(itInterfaceUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itInterface, result.Value);
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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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
            ExpectSystemServiceGetSystemReturns(inputParameters, operationError);

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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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
            Configure(fixture => fixture.Inject<IEnumerable<Guid>>(new Guid[]{overlapKey,uniqueKey,overlapKey}));
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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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
            ExpectSystemServiceGetSystemReturns(inputParameters, parentSystem);
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

        private void ExpectUpdateTaskRefsReturns(int systemId, List<int> expectedTaskRefIds, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateTaskRefs(systemId, It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(expectedTaskRefIds)))).Returns(result);
        }

        private void ExpectUpdateBusinessTypeReturns(int systemId, RightsHolderSystemCreationParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateBusinessType(systemId, inputParameters.BusinessTypeUuid)).Returns(result);
        }

        private void ExpectUpdateParentSystemReturns(int systemId, ItSystem parentSystem, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateParentSystem(systemId, parentSystem.Id)).Returns(result);
        }

        private void ExpectUpdateMainUrlReferenceReturns(int systemId, RightsHolderSystemCreationParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateMainUrlReference(systemId, inputParameters.UrlReference)).Returns(result);
        }

        private void ExpectUpdateDescriptionReturns(int systemId, RightsHolderSystemCreationParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateDescription(systemId, inputParameters.Description)).Returns(result);
        }

        private void ExpectUpdatePreviousNameReturns(int systemId, RightsHolderSystemCreationParameters inputParameters, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdatePreviousName(systemId, inputParameters.FormerName)).Returns(result);
        }

        private void ExpectUpdateRightsHolderReturns(int systemId, Guid rightsHolderUuid, Result<ItSystem, OperationError> result)
        {
            _itSystemServiceMock.Setup(x => x.UpdateRightsHolder(systemId, rightsHolderUuid)).Returns(result);
        }


        private void ExpectSystemServiceGetSystemReturns(RightsHolderSystemCreationParameters inputParameters, Result<ItSystem, OperationError> parentSystem)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(inputParameters.ParentSystemUuid.GetValueOrDefault())).Returns(parentSystem);
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

        private void ExpectHasSpecificAccessReturns(ItInterface itInterface, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(itInterface.ExhibitedBy.ItSystem.BelongsToId.Value, OrganizationRole.RightsHolderAccess))
                .Returns(value);
        }

        private void ExpectUserHasRightsHolderAccessReturns(bool value)
        {
            _userContextMock.Setup(x => x.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)).Returns(value);
        }
    }
}
