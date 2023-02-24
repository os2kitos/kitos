using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Interface.Write;
using Core.DomainModel.Events;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.RightsHolders
{
    public class ItInterfaceRightsHolderServiceTest : WithAutoFixture
    {
        private readonly ItInterfaceRightsHolderService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IItInterfaceService> _interfaceServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IGlobalAdminNotificationService> _globalAdminNotificationServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IOperationClock> _operationClockMock;
        private readonly Mock<IItInterfaceWriteService> _writeServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;

        public ItInterfaceRightsHolderServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _interfaceServiceMock = new Mock<IItInterfaceService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _logger = new Mock<ILogger>();
            _globalAdminNotificationServiceMock = new Mock<IGlobalAdminNotificationService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _operationClockMock = new Mock<IOperationClock>();

            _writeServiceMock = new Mock<IItInterfaceWriteService>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _sut = new ItInterfaceRightsHolderService(
                _userContextMock.Object,
                _organizationRepositoryMock.Object,
                _interfaceServiceMock.Object,
                _transactionManagerMock.Object,
                _logger.Object,
                _globalAdminNotificationServiceMock.Object,
                _userRepositoryMock.Object,
                _operationClockMock.Object,
                _writeServiceMock.Object,
                _domainEventsMock.Object,
                _databaseControlMock.Object);
        }

        [Fact]
        public void GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess_Returns_Forbidden_If_User_Does_Not_Have_RightsHolderAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);
            var refinements = new List<IDomainQuery<ItInterface>>();

            //Act
            var result = _sut.GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(refinements);

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
            _interfaceServiceMock.Setup(x => x.GetAvailableInterfaces(It.IsAny<IDomainQuery<ItInterface>>())).Returns(expectedResponse);
            var refinements = new List<IDomainQuery<ItInterface>>();

            //Act
            var result = _sut.GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(refinements);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedResponse, result.Value);
        }

        [Fact]
        public void Can_GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess_Applies_Refinements()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(true);
            var expectedItInterface = new ItInterface();
            var expectedResponse = new List<ItInterface>() { expectedItInterface };
            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(Many<int>());
            _interfaceServiceMock.Setup(x => x.GetAvailableInterfaces(It.IsAny<IDomainQuery<ItInterface>[]>())).Returns(expectedResponse.AsQueryable());

            var domainQuery = new Mock<IDomainQuery<ItInterface>>();
            domainQuery.Setup(x => x.Apply(It.IsAny<IQueryable<ItInterface>>())).Returns(new List<ItInterface>() { expectedItInterface }.AsQueryable());

            var refinements = new List<IDomainQuery<ItInterface>>();

            refinements.Add(domainQuery.Object);

            //Act
            var result = _sut.GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(refinements);

            //Assert
            Assert.True(result.Ok);
            var resultItInterface = Assert.Single(result.Value);
            Assert.Same(expectedItInterface, resultItInterface);
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
            _interfaceServiceMock.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(itInterface);
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
            _interfaceServiceMock.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(itInterface);

            //Act
            var result = _sut.GetInterfaceAsRightsHolder(itInterfaceUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itInterface, result.Value);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Ok()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var inputParameters = A<RightsHolderItInterfaceCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgId = A<int>();
            var itInterface = new ItInterface { Id = A<int>() };

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            _writeServiceMock.Setup(x => x.Update(itInterface.Uuid, It.Is<ItInterfaceWriteModelParameters>(p =>
                //First two are used during creation and reset before calling update
                p.Name.IsUnchanged &&
                p.InterfaceId.IsUnchanged &&
                //The rest of the params should be passed as-is
                p.Description == inputParameters.AdditionalValues.Description &&
                p.ExposingSystemUuid == inputParameters.AdditionalValues.ExposingSystemUuid &&
                p.Version == inputParameters.AdditionalValues.Version &&
                p.UrlReference == inputParameters.AdditionalValues.UrlReference
            ))).Returns(itInterface);

            //Act
            var result = _sut.CreateNewItInterface(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Forbidden_If_No_RightsHolderAccess_In_Target_Organization()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<RightsHolderItInterfaceCreationParameters>();
            var transactionMock = ExpectTransactionBegins();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, false);

            //Act
            var result = _sut.CreateNewItInterface(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Error_If_Create_Fails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<RightsHolderItInterfaceCreationParameters>();
            var transactionMock = ExpectTransactionBegins();

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, operationError);

            //Act
            var result = _sut.CreateNewItInterface(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Error_If_Update_After_Create_Fails()
        {
            //Arrange
            var rightsHolderUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<RightsHolderItInterfaceCreationParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>() };

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(rightsHolderUuid, new Organization { Id = orgId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            _writeServiceMock.Setup(x => x.Update(itInterface.Uuid, It.IsAny<ItInterfaceWriteModelParameters>())).Returns(operationError);

            //Act
            var result = _sut.CreateNewItInterface(rightsHolderUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void UpdateItInterface_Returns_Ok()
        {
            //Arrange
            var inputParameters = A<RightsHolderItInterfaceUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit(){ItSystem = new ItSystem(){BelongsToId = A<int>()}}};

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            _writeServiceMock.Setup(x => x.Update(itInterface.Uuid, It.Is<ItInterfaceWriteModelParameters>(p =>
                p.Name == inputParameters.Name &&
                p.InterfaceId == inputParameters.InterfaceId &&
                p.Description == inputParameters.Description &&
                p.ExposingSystemUuid == inputParameters.ExposingSystemUuid &&
                p.Version == inputParameters.Version &&
                p.UrlReference == inputParameters.UrlReference
            ))).Returns(itInterface);

            //Act
            var result = _sut.UpdateItInterface(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateItInterface_Returns_Error_If_WriteServiceUpdate_Fails()
        {
            //Arrange
            var inputParameters = new RightsHolderItInterfaceUpdateParameters()
            {
                ExposingSystemUuid = A<Guid>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit() { ItSystem = new ItSystem() { BelongsToId = A<int>() } } };
            var operationError = A<OperationError>();

            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectHasSpecificAccessReturns(itInterface, true);
            _writeServiceMock.Setup(x => x.Update(itInterface.Uuid, It.IsAny<ItInterfaceWriteModelParameters>())).Returns(operationError);

            //Act
            var result = _sut.UpdateItInterface(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void UpdateItInterface_Returns_Forbidden_If_Not_RightsHolder_Access()
        {
            //Arrange
            var inputParameters = A<RightsHolderItInterfaceUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit() { ItSystem = new ItSystem() { BelongsToId = A<int>() } } };

            ExpectHasSpecificAccessReturns(itInterface, false);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);

            //Act
            var result = _sut.UpdateItInterface(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void UpdateItInterface_Returns_BadState_If_ItInterface_Is_Disabled()
        {
            //Arrange
            var inputParameters = A<RightsHolderItInterfaceUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Disabled = true,Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit() { ItSystem = new ItSystem { BelongsToId = A<int>() } } };

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);

            //Act
            var result = _sut.UpdateItInterface(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Deactivate_Deactivates_System_And_Notifies_Global_Admins()
        {
            //Arrange
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };
            var userId = A<int>();

            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectDeactivateReturns(itInterface.Id, itInterface);
            _userContextMock.Setup(x => x.UserId).Returns(userId);
            _userRepositoryMock.Setup(x => x.GetById(userId)).Returns(new User { Email = A<string>() });

            //Act
            var result = _sut.Deactivate(itInterface.Uuid, reason);

            //Assert
            Assert.True(result.Ok);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Deactivate_Does_Not_Notify_Global_Admin_If_Deactivation_Fails()
        {
            //Arrange
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            var operationError = A<OperationError>();

            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectDeactivateReturns(itInterface.Id, operationError);

            //Act
            var result = _sut.Deactivate(itInterface.Uuid, reason);

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
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectHasSpecificAccessReturns(itInterface, false);

            //Act
            var result = _sut.Deactivate(itInterface.Uuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Deactivate_Does_Not_Notify_Global_Admin_If_Already_Deactivated()
        {
            //Arrange
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem }, Disabled = true };

            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectHasSpecificAccessReturns(itInterface, true);

            //Act
            var result = _sut.Deactivate(itInterface.Uuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Deactivate_Does_Not_Notify_Global_Admin_If_Get_System_Fails()
        {
            //Arrange
            var interfaceUuid = A<Guid>();
            var reason = A<string>();
            var transaction = ExpectTransactionBegins();
            var operationError = A<OperationError>();

            ExpectGetItInterfaceReturns(interfaceUuid, operationError);

            //Act
            var result = _sut.Deactivate(interfaceUuid, reason);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            _globalAdminNotificationServiceMock.Verify(x => x.Submit(It.IsAny<GlobalAdminNotification>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        private void ExpectDeactivateReturns(int itInterfaceId, Result<ItInterface, OperationError> result)
        {
            //TODO: Rewrite for write service integration
            _interfaceServiceMock.Setup(x => x.Deactivate(itInterfaceId)).Returns(result);
        }

        private void ExpectGetItInterfaceReturns(Guid itInterfaceUuid, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(result);
        }

        private void ExpectItInterfaceServiceCreateItInterfaceReturns(int orgId, RightsHolderItInterfaceCreationParameters inputParameters, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock
                .Setup(x => x.CreateNewItInterface(orgId, inputParameters.AdditionalValues.Name.NewValue, inputParameters.AdditionalValues.InterfaceId.NewValue, inputParameters.RightsHolderProvidedUuid, null))
                .Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid rightsHolderUuid, Maybe<Organization> organization)
        {
            _organizationRepositoryMock.Setup(x => x.GetByUuid(rightsHolderUuid)).Returns(organization);
        }

        private void ExpectUserHasRightsHolderAccessInOrganizationReturns(int orgId, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(orgId, OrganizationRole.RightsHolderAccess)).Returns(value);
        }

        private Mock<IDatabaseTransaction> ExpectTransactionBegins()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
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
