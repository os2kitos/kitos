using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Interface.Write;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Interface
{
    public class ItInterfaceWriteServiceTest : WithAutoFixture
    {
        private readonly ItInterfaceWriteService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<IItInterfaceService> _interfaceServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IGlobalAdminNotificationService> _globalAdminNotificationServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IOperationClock> _operationClockMock;

        public ItInterfaceWriteServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _interfaceServiceMock = new Mock<IItInterfaceService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _logger = new Mock<ILogger>();
            _globalAdminNotificationServiceMock = new Mock<IGlobalAdminNotificationService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _operationClockMock = new Mock<IOperationClock>();

            //TODO
            //_sut = new ItInterfaceRightsHolderService(
            //    _userContextMock.Object,
            //    _organizationRepositoryMock.Object,
            //    _itSystemServiceMock.Object,
            //    _interfaceServiceMock.Object,
            //    _transactionManagerMock.Object,
            //    _logger.Object,
            //    _globalAdminNotificationServiceMock.Object,
            //    _userRepositoryMock.Object,
            //    _operationClockMock.Object);
        }

        [Fact]
        public void Create_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgId = A<int>();
            var itInterface = new ItInterface { Id = A<int>() };
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);

            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            ExpectUpdateExposingSystemReturns(itInterface.Id, exposingSystem.Id, itInterface);
            ExpectUpdateVersionReturns(itInterface.Id, inputParameters, itInterface);
            ExpectUpdateDescriptionReturns(itInterface.Id, inputParameters, itInterface);
            ExpectUpdateUrlReferenceReturns(itInterface.Id, inputParameters, itInterface);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Create_Returns_BadInput_If_ExposingSystem_Has_No_Changes()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();

            inputParameters.ExposingSystemUuid = OptionalValueChange<Guid>.None;

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_BadInput_If_Name_Has_No_Changes()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();

            inputParameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_BadInput_If_GetRightsHolderOrganizationFails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();

            ExpectGetOrganizationReturns(organizationUuid, Maybe<Organization>.None);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_BadInput_If_GetExposingSystemFails_With_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = new ItInterfaceWriteModelParameters
            {
                ExposingSystemUuid = A<Guid>().AsChangedValue(),
                Name = A<string>().AsChangedValue(),
                InterfaceId = A<string>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>() };

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, new OperationError(OperationFailure.NotFound));

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_Forbidden_If_GetExposingSystemFails_With_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = new ItInterfaceWriteModelParameters
            {
                ExposingSystemUuid = A<Guid>().AsChangedValue(),
                Name = A<string>().AsChangedValue(),
                InterfaceId = A<string>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>() };

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, new OperationError(OperationFailure.Forbidden));

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_Forbidden_If_No_WriteAccess()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, false);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_Error_If_Create_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, operationError);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_Error_If_UpdateExposingSystem_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>(); var inputParameters = new ItInterfaceWriteModelParameters
            {
                ExposingSystemUuid = A<Guid>().AsChangedValue(),
                Name = A<string>().AsChangedValue(),
                InterfaceId = A<string>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>() };
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            ExpectUpdateExposingSystemReturns(itInterface.Id, exposingSystem.Id, operationError);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_Error_If_UpdateVersion_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>() };
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            ExpectUpdateExposingSystemReturns(itInterface.Id, exposingSystem.Id, itInterface);
            ExpectUpdateVersionReturns(itInterface.Id, inputParameters, operationError);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_Error_If_UpdateDescription_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>() };
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            ExpectUpdateExposingSystemReturns(itInterface.Id, exposingSystem.Id, itInterface);
            ExpectUpdateVersionReturns(itInterface.Id, inputParameters, itInterface);
            ExpectUpdateDescriptionReturns(itInterface.Id, inputParameters, operationError);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Create_Returns_Error_If_UpdateUrlReference_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var orgId = A<int>();
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>() };
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            var operationError = A<OperationError>();

            ExpectGetOrganizationReturns(organizationUuid, new Organization { Id = orgId });
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectUserHasRightsHolderAccessInOrganizationReturns(orgId, true);
            ExpectItInterfaceServiceCreateItInterfaceReturns(orgId, inputParameters, itInterface);
            ExpectUpdateExposingSystemReturns(itInterface.Id, exposingSystem.Id, itInterface);
            ExpectUpdateVersionReturns(itInterface.Id, inputParameters, itInterface);
            ExpectUpdateDescriptionReturns(itInterface.Id, inputParameters, itInterface);
            ExpectUpdateUrlReferenceReturns(itInterface.Id, inputParameters, operationError);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true)]
        [InlineData(false, false, false, false, false, true)]
        [InlineData(false, false, false, false, true, false)]
        [InlineData(false, false, false, true, false, false)]
        [InlineData(false, false, true, false, false, false)]
        [InlineData(false, true, false, false, false, false)]
        [InlineData(true, false, false, false, false, false)]
        [InlineData(false, false, false, false, false, false)]
        public void Update_Returns_Ok_And_Only_Updates_Parameters_With_Changes(
            bool withNameChange,
            bool withInterfaceIdChange,
            bool withExposingSystemChange,
            bool withVersionChange,
            bool withDescriptionChange,
            bool withUrlReferenceChange)
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters
            {
                Name = withNameChange ? A<string>().AsChangedValue() : OptionalValueChange<string>.None,
                InterfaceId = withInterfaceIdChange ? A<string>().AsChangedValue() : OptionalValueChange<string>.None,
                ExposingSystemUuid = withExposingSystemChange ? A<Guid>().AsChangedValue() : OptionalValueChange<Guid>.None,
                Version = withVersionChange ? A<string>().AsChangedValue() : OptionalValueChange<string>.None,
                Description = withDescriptionChange ? A<string>().AsChangedValue() : OptionalValueChange<string>.None,
                UrlReference = withUrlReferenceChange ? A<string>().AsChangedValue() : OptionalValueChange<string>.None,
            };
            var transactionMock = ExpectTransactionBegins();
            var originalExposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = originalExposingSystem }, ItInterfaceId = A<string>() };
            var newExposingSystem = new ItSystem { Id = A<int>(), Uuid = withExposingSystemChange ? inputParameters.ExposingSystemUuid.NewValue : A<Guid>(), BelongsToId = A<int>() };

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);

            if (withNameChange || withInterfaceIdChange)
                _interfaceServiceMock.Setup(x => x.UpdateNameAndInterfaceId(itInterface.Id, withNameChange ? inputParameters.Name.NewValue : itInterface.Name, withInterfaceIdChange ? inputParameters.InterfaceId.NewValue : itInterface.ItInterfaceId)).Returns(itInterface);

            if (withExposingSystemChange)
            {
                ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, newExposingSystem);
                ExpectUpdateExposingSystemReturns(itInterface.Id, newExposingSystem.Id, itInterface);
            }

            if (withVersionChange)
                ExpectUpdateVersionReturns(itInterface.Id, inputParameters, itInterface);

            if (withDescriptionChange)
                ExpectUpdateDescriptionReturns(itInterface.Id, inputParameters, itInterface);

            if (withUrlReferenceChange)
                ExpectUpdateUrlReferenceReturns(itInterface.Id, inputParameters, itInterface);


            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.Once);

            if (withNameChange || withInterfaceIdChange)
                _interfaceServiceMock.Verify(x => x.UpdateNameAndInterfaceId(itInterface.Id, withNameChange ? inputParameters.Name.NewValue : itInterface.Name, withInterfaceIdChange ? inputParameters.InterfaceId.NewValue : itInterface.ItInterfaceId), Times.Once);
            else
                _interfaceServiceMock.Verify(x => x.UpdateNameAndInterfaceId(itInterface.Id, It.IsAny<string>(), It.IsAny<string>()), Times.Never);


            if (withExposingSystemChange)
                _interfaceServiceMock.Verify(x => x.UpdateExposingSystem(itInterface.Id, newExposingSystem.Id), Times.Once);
            else
                _interfaceServiceMock.Verify(x => x.UpdateExposingSystem(itInterface.Id, It.IsAny<int?>()), Times.Never);


            if (withVersionChange)
                _interfaceServiceMock.Verify(x => x.UpdateVersion(itInterface.Id, inputParameters.Version.NewValue), Times.Once);
            else
                _interfaceServiceMock.Verify(x => x.UpdateVersion(itInterface.Id, It.IsAny<string>()), Times.Never);


            if (withDescriptionChange)
                _interfaceServiceMock.Verify(x => x.UpdateDescription(itInterface.Id, inputParameters.Description.NewValue), Times.Once);
            else
                _interfaceServiceMock.Verify(x => x.UpdateDescription(itInterface.Id, It.IsAny<string>()), Times.Never);


            if (withUrlReferenceChange)
                _interfaceServiceMock.Verify(x => x.UpdateUrlReference(itInterface.Id, inputParameters.UrlReference.NewValue), Times.Once);
            else
                _interfaceServiceMock.Verify(x => x.UpdateUrlReference(itInterface.Id, It.IsAny<string>()), Times.Never);

        }

        [Fact]
        public void Update_Returns_BadInput_If_GetExposingSystemFails_With_NotFound()
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters
            {
                ExposingSystemUuid = A<Guid>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, new OperationError(OperationFailure.NotFound));

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Forbidden_If_GetExposingSystemFails_With_Forbidden()
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters
            {
                ExposingSystemUuid = A<Guid>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, new OperationError(OperationFailure.Forbidden));

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Error_If_GetInterface_Fails()
        {
            //Arrange
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>() };
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>() };

            var operationError = A<OperationError>();

            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectGetItInterfaceReturns(itInterface.Uuid, operationError);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Forbidden_If_Not_RightsHolder_Access()
        {
            //Arrange
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            ExpectHasSpecificAccessReturns(itInterface, false);
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_BadState_If_ItInterface_Is_Disabled()
        {
            //Arrange
            var inputParameters = A<ItInterfaceWriteModelParameters>();
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem }, Disabled = true };

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Error_If_UpdateNameAndInterfaceId_Fails()
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters()
            {
                Name = A<string>().AsChangedValue(),
                InterfaceId = A<string>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            var operationError = A<OperationError>();

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectUpdateNameAndInterfaceIdReturns(itInterface.Id, inputParameters, operationError);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Error_If_UpdateExposingSystem_Fails()
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters()
            {
                ExposingSystemUuid = A<Guid>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            var operationError = A<OperationError>();

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetSystemReturns(inputParameters.ExposingSystemUuid.NewValue, exposingSystem);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectUpdateExposingSystemReturns(itInterface.Id, exposingSystem.Id, operationError);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Error_If_UpdateVersion_Fails()
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters()
            {
                Version = A<string>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            var operationError = A<OperationError>();

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectUpdateVersionReturns(itInterface.Id, inputParameters, operationError);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Error_If_UpdateDescription_Fails()
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters()
            {
                Description = A<string>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            var operationError = A<OperationError>();

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectUpdateDescriptionReturns(itInterface.Id, inputParameters, operationError);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Update_Returns_Error_If_UpdateUrlReference_Fails()
        {
            //Arrange
            var inputParameters = new ItInterfaceWriteModelParameters()
            {
                UrlReference = A<string>().AsChangedValue()
            };
            var transactionMock = ExpectTransactionBegins();
            var exposingSystem = new ItSystem { Id = A<int>(), Uuid = A<Guid>(), BelongsToId = A<int>() };
            var itInterface = new ItInterface { Id = A<int>(), Uuid = A<Guid>(), ExhibitedBy = new ItInterfaceExhibit { ItSystem = exposingSystem } };

            var operationError = A<OperationError>();

            ExpectHasSpecificAccessReturns(itInterface, true);
            ExpectGetItInterfaceReturns(itInterface.Uuid, itInterface);
            ExpectUpdateUrlReferenceReturns(itInterface.Id, inputParameters, operationError);

            //Act
            var result = _sut.Update(itInterface.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        //TODO: Test delete       

        private void ExpectGetItInterfaceReturns(Guid itInterfaceUuid, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock.Setup(x => x.GetInterface(itInterfaceUuid)).Returns(result);
        }

        private void ExpectUpdateExposingSystemReturns(int interfaceId, int exposingSystemId, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock.Setup(x => x.UpdateExposingSystem(interfaceId, exposingSystemId)).Returns(result);
        }

        private void ExpectUpdateNameAndInterfaceIdReturns(int interfaceId, ItInterfaceWriteModelParameters inputParameters, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock.Setup(x => x.UpdateNameAndInterfaceId(interfaceId, inputParameters.Name.NewValue, inputParameters.InterfaceId.NewValue)).Returns(result);
        }

        private void ExpectUpdateUrlReferenceReturns(int interfaceId, ItInterfaceWriteModelParameters inputParameters, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock.Setup(x => x.UpdateUrlReference(interfaceId, inputParameters.UrlReference.NewValue)).Returns(result);
        }

        private void ExpectUpdateVersionReturns(int interfaceId, ItInterfaceWriteModelParameters inputParameters, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock.Setup(x => x.UpdateVersion(interfaceId, inputParameters.Version.NewValue)).Returns(result);
        }

        private void ExpectUpdateDescriptionReturns(int interfaceId, ItInterfaceWriteModelParameters inputParameters, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock.Setup(x => x.UpdateDescription(interfaceId, inputParameters.Description.NewValue)).Returns(result);
        }

        private void ExpectItInterfaceServiceCreateItInterfaceReturns(int orgId, ItInterfaceWriteModelParameters inputParameters, Result<ItInterface, OperationError> result)
        {
            _interfaceServiceMock
                .Setup(x => x.CreateNewItInterface(orgId, inputParameters.Name.NewValue, inputParameters.InterfaceId.NewValue, null, null))
                .Returns(result);
        }

        private void ExpectGetSystemReturns(Guid systemUuid, Result<ItSystem, OperationError> system)
        {
            _itSystemServiceMock.Setup(x => x.GetSystem(systemUuid)).Returns(system);
        }

        private void ExpectGetOrganizationReturns(Guid organizationUuid, Maybe<Organization> organization)
        {
            _organizationRepositoryMock.Setup(x => x.GetByUuid(organizationUuid)).Returns(organization);
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
    }
}
