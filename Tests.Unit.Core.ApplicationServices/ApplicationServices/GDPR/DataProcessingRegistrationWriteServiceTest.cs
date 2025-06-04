using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.GDPR.Write;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationWriteServiceTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationWriteService _sut;
        private readonly Mock<IDataProcessingRegistrationApplicationService> _dprServiceMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IReferenceService> _referenceServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;
        private readonly Mock<IAssignmentUpdateService> _assignmentUpdateServiceMock;

        public DataProcessingRegistrationWriteServiceTest()
        {
            _dprServiceMock = new Mock<IDataProcessingRegistrationApplicationService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _referenceServiceMock = new Mock<IReferenceService>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _assignmentUpdateServiceMock = new Mock<IAssignmentUpdateService>();
            _sut = new DataProcessingRegistrationWriteService(
                _dprServiceMock.Object,
                _identityResolverMock.Object,
                _referenceServiceMock.Object,
                Mock.Of<ILogger>(),
                _domainEventsMock.Object,
                _transactionManagerMock.Object,
                _databaseControlMock.Object,
                _assignmentUpdateServiceMock.Object);
        }
        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            var outerFixture = new Fixture();

            //Ensure operation errors are always auto-created WITH both failure and message
            fixture.Register(() => new OperationError(outerFixture.Create<string>(), outerFixture.Create<OperationFailure>()));
        }

        [Fact]
        public void Can_Create_With_Name_Only()
        {
            //Arrange
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites();

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_If_Dpr_Creation_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();
            var operationError = A<OperationError>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);
            ExpectCreateDataProcessingRegistrationReturns(orgDbId, parameters, parameters.Name.NewValue, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_If_Name_Is_Not_Provided()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = OptionalValueChange<string>.None
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Name must be provided", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_If_Organization_Id_Resolution_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var transaction = ExpectTransaction();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Unable to resolve Organization with uuid", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Can_Update_Name()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Id = A<int>()
            };
            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();

            ExpectGetDataProcessingRegistrationReturns(dprUuid, dataProcessingRegistration);
            ExpectUpdateNameReturns(dataProcessingRegistration.Id, parameters.Name.NewValue, dataProcessingRegistration);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration, result.Value);
            AssertTransactionCommitted(transaction);
            _domainEventsMock.Verify(x => x.Raise(It.IsAny<EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot>>()), Times.Once);
        }

        [Fact]
        public void Update_Name_Does_Not_Change_Anything_If_No_NameChange_Is_Defined()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = OptionalValueChange<string>.None
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Id = A<int>()
            };
            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();

            ExpectGetDataProcessingRegistrationReturns(dprUuid, dataProcessingRegistration);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration, result.Value);
            AssertTransactionCommitted(transaction);
            _dprServiceMock.Verify(x => x.UpdateName(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Cannot_Update_Name_If_NameUpdate_Fails()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Id = A<int>()
            };
            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();
            var operationError = A<OperationError>();

            ExpectGetDataProcessingRegistrationReturns(dprUuid, dataProcessingRegistration);
            ExpectUpdateNameReturns(dataProcessingRegistration.Id, parameters.Name.NewValue, operationError);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Update_Name_If_Dpr_Resolution_Fails()
        {
            //Arrange
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue()
            };

            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();
            var operationError = A<OperationError>();

            ExpectGetDataProcessingRegistrationReturns(dprUuid, operationError);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_With_GeneralData_DataResponsible()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.AssignDataResponsible(createdRegistration.Id, responsibleId)).Returns(new DataProcessingDataResponsibleOption());

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_GeneralData_With_Null_DataResponsible()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.ClearDataResponsible(createdRegistration.Id)).Returns(new DataProcessingDataResponsibleOption());

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_GeneralData_With_Null_DataResponsible_If_DprService_Responds_With_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.ClearDataResponsible(createdRegistration.Id)).Returns(new OperationError(OperationFailure.BadState));

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_With_Null_DataResponsible_If_DprService_Fails_With_AnythingBut_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();
            var clearError = new OperationError(EnumRange.AllExcept(OperationFailure.BadState).RandomItem());

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.ClearDataResponsible(createdRegistration.Id)).Returns(clearError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, clearError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_DataResponsible_If_DataResponsible_Id_Cannot_Be_Resolved()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, _, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingDataResponsibleOption>(generalData.DataResponsibleUuid.NewValue, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Data responsible option with uuid", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_DataResponsible_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = OptionalValueChange<Guid?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.AssignDataResponsible(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_DataResponsibleRemark(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleRemark = (inputIsNull ? null : A<string>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            _dprServiceMock.Setup(x => x.UpdateDataResponsibleRemark(createdRegistration.Id, generalData.DataResponsibleRemark.NewValue)).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_DataResponsibleRemark_If_Update_Fails()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleRemark = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateDataResponsibleRemark(createdRegistration.Id, generalData.DataResponsibleRemark.NewValue)).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_DataResponsibleRemark_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateDataResponsibleRemark(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_IsAgreementConcluded(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcluded = (inputIsNull ? (YesNoIrrelevantOption?)null : A<YesNoIrrelevantOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            _dprServiceMock.Setup(x => x.UpdateIsAgreementConcluded(createdRegistration.Id, generalData.IsAgreementConcluded.NewValue.GetValueOrDefault(YesNoIrrelevantOption.UNDECIDED))).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_IsAgreementConcluded_If_Update_Fails()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcluded = A<YesNoIrrelevantOption?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateIsAgreementConcluded(createdRegistration.Id, generalData.IsAgreementConcluded.NewValue.GetValueOrDefault(YesNoIrrelevantOption.UNDECIDED))).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_IsAgreementConcluded_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcluded = OptionalValueChange<YesNoIrrelevantOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateIsAgreementConcluded(It.IsAny<int>(), It.IsAny<YesNoIrrelevantOption>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_IsAgreementConcludedRemark(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcludedRemark = (inputIsNull ? null : A<string>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            _dprServiceMock.Setup(x => x.UpdateAgreementConcludedRemark(createdRegistration.Id, generalData.IsAgreementConcludedRemark.NewValue)).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_IsAgreementConcludedRemark_If_Update_Fails()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcludedRemark = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateAgreementConcludedRemark(createdRegistration.Id, generalData.IsAgreementConcludedRemark.NewValue)).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_IsAgreementConcludedRemark_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                IsAgreementConcludedRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateAgreementConcludedRemark(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_AgreementConcludedAt(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                AgreementConcludedAt = (inputIsNull ? (DateTime?)null : A<DateTime>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            _dprServiceMock.Setup(x => x.UpdateAgreementConcludedAt(createdRegistration.Id, generalData.AgreementConcludedAt.NewValue)).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_AgreementConcludedAt_If_Update_Fails()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                AgreementConcludedAt = ((DateTime?)A<DateTime>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateAgreementConcludedAt(createdRegistration.Id, generalData.AgreementConcludedAt.NewValue)).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_AgreementConcludedAt_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                AgreementConcludedAt = OptionalValueChange<DateTime?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateAgreementConcludedAt(It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [Fact]
        public void Can_Create_With_GeneralData_BasisForTransfer()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.AssignBasisForTransfer(createdRegistration.Id, responsibleId)).Returns(new DataProcessingBasisForTransferOption());

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_GeneralData_With_Null_BasisForTransfer()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.ClearBasisForTransfer(createdRegistration.Id)).Returns(new DataProcessingBasisForTransferOption());

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_GeneralData_With_Null_BasisForTransfer_If_DprService_Responds_With_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.ClearBasisForTransfer(createdRegistration.Id)).Returns(new OperationError(OperationFailure.BadState));

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_With_Null_BasisForTransfer_If_DprService_Fails_With_AnythingBut_BadState()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = ((Guid?)null).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            var responsibleId = A<int>();
            var clearError = new OperationError(EnumRange.AllExcept(OperationFailure.BadState).RandomItem());

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, responsibleId);
            _dprServiceMock.Setup(x => x.ClearBasisForTransfer(createdRegistration.Id)).Returns(clearError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, clearError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_BasisForTransfer_If_BasisForTransfer_Id_Cannot_Be_Resolved()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = new Guid?(A<Guid>()).AsChangedValue()
            };
            var (organizationUuid, parameters, _, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(generalData.BasisForTransferUuid.NewValue, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Basis for transfer option with uuid", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_BasisForTransfer_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                BasisForTransferUuid = OptionalValueChange<Guid?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.AssignBasisForTransfer(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_TransferToInsecureThirdCountries(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                TransferToInsecureThirdCountries = (inputIsNull ? (YesNoUndecidedOption?)null : A<YesNoUndecidedOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            _dprServiceMock.Setup(x => x.UpdateTransferToInsecureThirdCountries(createdRegistration.Id, generalData.TransferToInsecureThirdCountries.NewValue.GetValueOrDefault(YesNoUndecidedOption.Undecided))).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_TransferToInsecureThirdCountries_If_Update_Fails()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                TransferToInsecureThirdCountries = A<YesNoUndecidedOption?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateTransferToInsecureThirdCountries(createdRegistration.Id, generalData.TransferToInsecureThirdCountries.NewValue.GetValueOrDefault(YesNoUndecidedOption.Undecided))).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_TransferToInsecureThirdCountries_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                TransferToInsecureThirdCountries = OptionalValueChange<YesNoUndecidedOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateTransferToInsecureThirdCountries(It.IsAny<int>(), It.IsAny<YesNoUndecidedOption>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_HasSubDataProcessors(bool inputIsNull)
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                HasSubDataProcessors = (inputIsNull ? (YesNoUndecidedOption?)null : A<YesNoUndecidedOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            _dprServiceMock.Setup(x => x.SetSubDataProcessorsState(createdRegistration.Id, generalData.HasSubDataProcessors.NewValue.GetValueOrDefault(YesNoUndecidedOption.Undecided))).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_HasSubDataProcessors_If_Update_Fails()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                HasSubDataProcessors = A<YesNoUndecidedOption?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.SetSubDataProcessorsState(createdRegistration.Id, generalData.HasSubDataProcessors.NewValue.GetValueOrDefault(YesNoUndecidedOption.Undecided))).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Create_With_GeneralData_HasSubDataProcessors_Set_To_NoChanges()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                HasSubDataProcessors = OptionalValueChange<YesNoUndecidedOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.SetSubDataProcessorsState(It.IsAny<int>(), It.IsAny<YesNoUndecidedOption>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_InsecureCountriesSubjectToDataTransfer(bool hasInsecureCountries)
        {
            //Arrange
            var inputUuids = hasInsecureCountries ? Many<Guid>().ToList() : new List<Guid>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                InsecureCountriesSubjectToDataTransferUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectUpdateMultiAssignmentReturns<int, DataProcessingCountryOption>(createdRegistration, inputUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_InsecureCountriesSubjectToDataTransfer_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                InsecureCountriesSubjectToDataTransferUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, DataProcessingCountryOption>(createdRegistration, inputUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_DataProcessor(bool hasDataProcessors)
        {
            //Arrange
            var inputUuids = hasDataProcessors ? Many<Guid>().ToList() : new List<Guid>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataProcessorUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, inputUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_DataProcessor_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataProcessorUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, inputUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_SubDataProcessor(bool hasSubDataProcessors)
        {
            //Arrange
            var sdpInputs = hasSubDataProcessors ? Many<SubDataProcessorParameter>().ToList() : new List<SubDataProcessorParameter>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);
            sdpInputs.ForEach(sdp =>
            {
                //All ids return correct data
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(sdp.OrganizationUuid, A<int>());
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, A<int>());
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.ForEach(sdp =>
            {
                //All ids return correct data
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(sdp.OrganizationUuid, A<int>());
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, A<int>());
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_BasisForTransferIdLookup_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.Take(1).ToList().ForEach(sdp =>
            {
                //All ids return correct data
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, Maybe<int>.None);
                operationError = new OperationError($"Provided id for basis for transfer {sdp.BasisForTransferOptionUuid.Value} does not point to a valid entity", OperationFailure.BadInput);
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_InsecureCountry_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.Take(1).ToList().ForEach(sdp =>
            {
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue)
                {
                    ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, Maybe<int>.None);
                    operationError = new OperationError($"Provided id for country {sdp.InsecureCountrySubjectToDataTransferUuid.Value} does not point to a valid entity", OperationFailure.BadInput);
                }
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_SubDataProcessor_If_Organization_Fails()
        {
            //Arrange
            var sdpInputs = Many<SubDataProcessorParameter>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessors = sdpInputs.AsEnumerable().FromNullable().AsChangedValue()
            };

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var operationError = A<OperationError>();
            sdpInputs.Take(1).ToList().ForEach(sdp =>
            {
                if (sdp.BasisForTransferOptionUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingBasisForTransferOption>(sdp.BasisForTransferOptionUuid.Value, A<int>());
                if (sdp.InsecureCountrySubjectToDataTransferUuid.HasValue) ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(sdp.InsecureCountrySubjectToDataTransferUuid.Value, A<int>());
                ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(sdp.OrganizationUuid, Maybe<int>.None);
                operationError = new OperationError($"Provided org id {sdp.OrganizationUuid} does not point to a valid entity", OperationFailure.BadInput);
            });
            ExpectUpdateMultiAssignmentReturns<int, Organization>(createdRegistration, sdpInputs.Select(x => x.OrganizationUuid).FromNullable(), operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_GeneralData_MainContract(bool hasMainContract)
        {
            //Arrange
            var inputUuids = hasMainContract ? A<Guid?>() : null;
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                MainContractUuid = inputUuids.AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var contract = createdRegistration.AssociatedContracts.FirstOrDefault();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(generalData.MainContractUuid.NewValue, contract.Id);
            if (hasMainContract)
            {
                ExpectUpdateMainContractReturns(createdRegistration.Id, contract.Id, createdRegistration);
            }
            else
            {
                ExpectRemoveMainContractReturns(createdRegistration.Id, createdRegistration);
            }

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Create_With_GeneralData_MainContract_Returns_OperationError_When_UpdateMainContract_Fails()
        {
            //Arrange
            var inputUuids = A<Guid?>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                MainContractUuid = inputUuids.AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var contract = createdRegistration.AssociatedContracts.First();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(generalData.MainContractUuid.NewValue, contract.Id);
            var expectedError = A<OperationError>();
            ExpectUpdateMainContractReturns(createdRegistration.Id, contract.Id, expectedError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedError.FailureType, result.Error.FailureType);
            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public void Create_With_GeneralData_MainContract_Returns_OperationError_When_RemoveMainContract_Fails()
        {
            //Arrange
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                MainContractUuid = OptionalValueChange<Guid?>.With(null),
            };
            var (organizationUuid, parameters, createdRegistration, _) = SetupCreateScenarioPrerequisites(generalData: generalData);

            var expectedError = A<OperationError>();
            ExpectRemoveMainContractReturns(createdRegistration.Id, expectedError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(expectedError.FailureType, result.Error.FailureType);
            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public void Cannot_Create_With_GeneralData_MainContract_If_MainContract_Id_Cannot_Be_Resolved()
        {
            //Arrange
            var inputUuids = A<Guid?>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                MainContractUuid = inputUuids.AsChangedValue()
            };
            var (organizationUuid, parameters, _, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(generalData.MainContractUuid.NewValue, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownErrorDetails(result, $"It contract with uuid {generalData.MainContractUuid.NewValue} could not be found", OperationFailure.BadInput, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_SystemUsages(bool hasUsages)
        {
            //Arrange
            var usageUuids = hasUsages ? Many<Guid>().ToList() : new List<Guid>();
            var (organizationUuid, parameters, registration, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);

            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_SystemUsages_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var usageUuids = Many<Guid>().ToList();
            var (organizationUuid, parameters, registration, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Update_With_SystemUsages_If_Usage_Already_Assigned()
        {
            //Arrange
            var usage = new ItSystemUsage() { Id = A<int>(), Uuid = A<Guid>() };
            var usageUuids = new List<Guid> { usage.Uuid };
            var (organizationUuid, parameters, registration, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);
            registration.SystemUsages.Add(usage);

            ExpectGetDataProcessingRegistrationReturns(registration.Uuid, registration);
            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, Maybe<OperationError>.None);
            parameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Update(registration.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Update_With_SystemUsages_To_Remove_Usage()
        {
            //Arrange
            var usage = new ItSystemUsage() { Id = A<int>(), Uuid = A<Guid>() };
            var usageUuids = new List<Guid>();
            var (organizationUuid, parameters, registration, transaction) = SetupCreateScenarioPrerequisites(systemUsageUuids: usageUuids);
            registration.SystemUsages.Add(usage);

            ExpectGetDataProcessingRegistrationReturns(registration.Uuid, registration);
            ExpectUpdateMultiAssignmentReturns<int, ItSystemUsage>(registration, usageUuids, Maybe<OperationError>.None);
            parameters.Name = OptionalValueChange<string>.None;

            //Act
            var result = _sut.Update(registration.Uuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }


        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var uuid = A<Guid>();
            var resolvedDbId = A<int>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistration>(uuid, resolvedDbId);
            _dprServiceMock.Setup(x => x.Delete(resolvedDbId)).Returns(new DataProcessingRegistration());

            //Act
            var result = _sut.Delete(uuid);

            //Assert
            Assert.True(result.IsNone, "No errors should occur during deletion");
        }

        [Fact]
        public void Cannot_Delete_If_Deletion_Fails()
        {
            //Arrange
            var uuid = A<Guid>();
            var resolvedDbId = A<int>();
            var operationError = A<OperationError>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistration>(uuid, resolvedDbId);
            _dprServiceMock.Setup(x => x.Delete(resolvedDbId)).Returns(operationError);

            //Act
            var result = _sut.Delete(uuid);

            //Assert
            Assert.True(result.HasValue);
            Assert.Same(operationError, result.Value);
        }

        [Fact]
        public void Cannot_Delete_If_IdentityResolutionFails()
        {
            //Arrange
            var uuid = A<Guid>();
            var resolvedDbId = A<int>();
            var operationError = A<OperationError>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistration>(uuid, Maybe<int>.None);

            //Act
            var result = _sut.Delete(uuid);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_Oversight_OversightOptions(bool hasOversightOptions)
        {
            //Arrange
            var inputUuids = hasOversightOptions ? Many<Guid>().ToList() : new List<Guid>();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            ExpectUpdateMultiAssignmentReturns<int, DataProcessingOversightOption>(createdRegistration, inputUuids, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_Oversight_OversightOptions_If_UpdateMultiAssignment_Fails()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            ExpectUpdateMultiAssignmentReturns<int, DataProcessingOversightOption>(createdRegistration, inputUuids, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightOptionsRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionsRemark = (inputIsNull ? null : A<string>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            _dprServiceMock.Setup(x => x.UpdateOversightOptionRemark(createdRegistration.Id, oversightData.OversightOptionsRemark.NewValue)).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_OversightData_OversightOptionsRemark_If_Update_Fails()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionsRemark = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateOversightOptionRemark(createdRegistration.Id, oversightData.OversightOptionsRemark.NewValue)).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightOptionsRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightOptionsRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateOversightOptionRemark(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightInterval(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightInterval = (inputIsNull ? (YearMonthIntervalOption?)null : A<YearMonthIntervalOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            _dprServiceMock.Setup(x => x.UpdateOversightInterval(createdRegistration.Id, oversightData.OversightInterval.NewValue.GetValueOrDefault(YearMonthIntervalOption.Undecided))).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_OversightData_OversightInterval_If_Update_Fails()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightInterval = A<YearMonthIntervalOption?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateOversightInterval(createdRegistration.Id, oversightData.OversightInterval.NewValue.GetValueOrDefault(YearMonthIntervalOption.Undecided))).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightInterval_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightInterval = OptionalValueChange<YearMonthIntervalOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateOversightInterval(It.IsAny<int>(), It.IsAny<YearMonthIntervalOption>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightIntervalRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightIntervalRemark = (inputIsNull ? null : A<string>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            _dprServiceMock.Setup(x => x.UpdateOversightIntervalRemark(createdRegistration.Id, oversightData.OversightIntervalRemark.NewValue)).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_OversightData_OversightIntervalRemark_If_Update_Fails()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightIntervalRemark = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateOversightIntervalRemark(createdRegistration.Id, oversightData.OversightIntervalRemark.NewValue)).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightIntervalRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightIntervalRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateOversightIntervalRemark(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_OversightIntervalRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters
            {
                OversightScheduledInspectionDate = (inputIsNull ? null : A<DateTime?>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            _dprServiceMock.Setup(x => x.UpdateOversightScheduledInspectionDate(createdRegistration.Id, oversightData.OversightScheduledInspectionDate.NewValue)).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_OversightIntervalRemark_If_Update_Fails()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightScheduledInspectionDate = A<DateTime?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateOversightScheduledInspectionDate(createdRegistration.Id, oversightData.OversightScheduledInspectionDate.NewValue.GetValueOrDefault())).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_OversightIntervalRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightScheduledInspectionDate = OptionalValueChange<DateTime?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateOversightScheduledInspectionDate(It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_IsOversightCompleted(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                IsOversightCompleted = (inputIsNull ? (YesNoUndecidedOption?)null : A<YesNoUndecidedOption>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            _dprServiceMock.Setup(x => x.UpdateIsOversightCompleted(createdRegistration.Id, oversightData.IsOversightCompleted.NewValue.GetValueOrDefault(YesNoUndecidedOption.Undecided))).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_OversightData_IsOversightCompleted_If_Update_Fails()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                IsOversightCompleted = A<YesNoUndecidedOption?>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateIsOversightCompleted(createdRegistration.Id, oversightData.IsOversightCompleted.NewValue.GetValueOrDefault(YesNoUndecidedOption.Undecided))).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_With_OversightData_IsOversightCompleted_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                IsOversightCompleted = OptionalValueChange<YesNoUndecidedOption?>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateIsOversightCompleted(It.IsAny<int>(), It.IsAny<YesNoUndecidedOption>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Create_With_OversightData_OversightCompletedRemark(bool inputIsNull)
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightCompletedRemark = (inputIsNull ? null : A<string>()).AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            _dprServiceMock.Setup(x => x.UpdateOversightCompletedRemark(createdRegistration.Id, oversightData.OversightCompletedRemark.NewValue)).Returns(createdRegistration);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_OversightData_OversightCompletedRemark_If_Update_Fails()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightCompletedRemark = A<string>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var operationError = A<OperationError>();
            _dprServiceMock.Setup(x => x.UpdateOversightCompletedRemark(createdRegistration.Id, oversightData.OversightCompletedRemark.NewValue)).Returns(operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightCompletedRemark_Set_To_NoChanges()
        {
            //Arrange
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightCompletedRemark = OptionalValueChange<string>.None
            };
            var (organizationUuid, parameters, _, _) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            _dprServiceMock.Verify(x => x.UpdateOversightCompletedRemark(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightDates()
        {
            //Arrange
            var dates = Many<UpdatedDataProcessingRegistrationOversightDate>().ToList();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightDates = dates.FromNullable<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var oversightDatesMap =
                dates
                    .ToDictionary(
                        x => x,
                        x => new DataProcessingRegistrationOversightDate { Id = A<int>(), OversightDate = x.CompletedAt, OversightRemark = x.Remark });

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Setup(x => x.AssignOversightDate(createdRegistration.Id, oversightDate.CompletedAt, oversightDate.Remark)).Returns(oversightDatesMap[oversightDate]);
            }

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Verify(x => x.AssignOversightDate(createdRegistration.Id, oversightDate.CompletedAt, oversightDate.Remark), Times.Once);
            }

            _dprServiceMock.Verify(x => x.RemoveOversightDate(createdRegistration.Id, It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightDates_Removes_Existing_If_None()
        {
            //Arrange
            var dates = Many<UpdatedDataProcessingRegistrationOversightDate>().ToList();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightDates = Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>.None.AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var oversightDatesMap =
                dates
                    .ToDictionary(
                        x => x,
                        x => new DataProcessingRegistrationOversightDate { Id = A<int>(), OversightDate = x.CompletedAt, OversightRemark = x.Remark });

            createdRegistration.OversightDates = dates.Select(x => oversightDatesMap[x]).ToList();

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Setup(x => x.RemoveOversightDate(createdRegistration.Id, oversightDatesMap[oversightDate].Id)).Returns(oversightDatesMap[oversightDate]);
            }

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Verify(x => x.RemoveOversightDate(createdRegistration.Id, oversightDatesMap[oversightDate].Id), Times.Once);
            }

            _dprServiceMock.Verify(x => x.AssignOversightDate(createdRegistration.Id, It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightDates_Updates_Existing_If_Any()
        {
            //Arrange
            var dates = Many<UpdatedDataProcessingRegistrationOversightDate>().ToList();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightDates = dates.FromNullable<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            var oversightDatesMap =
                dates
                    .ToDictionary(
                        x => x,
                        x => new DataProcessingRegistrationOversightDate { Id = A<int>(), OversightDate = x.CompletedAt, OversightRemark = x.Remark });

            createdRegistration.OversightDates = dates.Select(x => oversightDatesMap[x]).ToList();

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Setup(x => x.RemoveOversightDate(createdRegistration.Id, oversightDatesMap[oversightDate].Id)).Returns(oversightDatesMap[oversightDate]);
            }

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Setup(x => x.AssignOversightDate(createdRegistration.Id, oversightDate.CompletedAt, oversightDate.Remark)).Returns(oversightDatesMap[oversightDate]);
            }

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Verify(x => x.RemoveOversightDate(createdRegistration.Id, oversightDatesMap[oversightDate].Id), Times.Once);
            }

            foreach (var oversightDate in dates)
            {
                _dprServiceMock.Verify(x => x.AssignOversightDate(createdRegistration.Id, oversightDate.CompletedAt, oversightDate.Remark), Times.Once);
            }
        }

        [Fact]
        public void Can_Create_With_OversightData_OversightDates_Set_To_NoChanges()
        {
            //Arrange
            var dates = Many<UpdatedDataProcessingRegistrationOversightDate>().ToList();
            var oversightData = new UpdatedDataProcessingRegistrationOversightDataParameters()
            {
                OversightDates = OptionalValueChange<Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>>.None
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(oversightData: oversightData);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);

            _dprServiceMock.Verify(x => x.RemoveOversightDate(createdRegistration.Id, It.IsAny<int>()), Times.Never);
            _dprServiceMock.Verify(x => x.AssignOversightDate(createdRegistration.Id, It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);

        }

        [Fact]
        public void Can_Create_With_Roles()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };
            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(roles: roles);
            var right = CreateRight(createdRegistration, roleUuid, roleId, userUuid, userId);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);
            ExpectRoleAssignmentReturns(createdRegistration, roleId, userId, right);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
            _dprServiceMock.Verify(x => x.AssignRole(createdRegistration.Id, roleId, userId), Times.Once);
            _dprServiceMock.Verify(x => x.RemoveRole(createdRegistration.Id, It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Can_Create_With_No_Roles()
        {
            //Arrange
            var roles = new UpdatedDataProcessingRegistrationRoles() { UserRolePairs = Maybe<IEnumerable<UserRolePair>>.None.AsChangedValue() };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(roles: roles);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
            _dprServiceMock.Verify(x => x.AssignRole(createdRegistration.Id, It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _dprServiceMock.Verify(x => x.RemoveRole(createdRegistration.Id, It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_RoleUuid_Not_Found()
        {
            //Arrange
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(roles: roles);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, Maybe<int>.None);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, $"Could not find role with Uuid: {roleUuid}", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_UserUuid_Not_Found()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(roles: roles);


            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, Maybe<int>.None);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, $"Could not find user with Uuid: {userUuid}", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_Role_Assignment_Fails()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(roles: roles);

            var error = A<OperationError>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);
            ExpectRoleAssignmentReturns(createdRegistration, roleId, userId, error);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, $"Failed to assign role with Uuid: {roleUuid} from user with Uuid: {userUuid}, with following error message:", error.FailureType, transaction);
        }

        [Fact]
        public void Cannot_Create_With_Roles_If_Duplicates()
        {
            //Arrange
            var roleId = A<int>();
            var roleUuid = A<Guid>();
            var userId = A<int>();
            var userUuid = A<Guid>();

            var rolePairs = new List<UserRolePair>() { CreateUserRolePair(roleUuid, userUuid), CreateUserRolePair(roleUuid, userUuid) };

            var roles = CreateUpdatedDataProcessingRegistrationRoles(rolePairs);

            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(roles: roles);
            var right = CreateRight(createdRegistration, roleUuid, roleId, userUuid, userId);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(roleUuid, roleId);
            ExpectRoleAssignmentReturns(createdRegistration, roleId, userId, right);

            //Act
            var createResult = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(createResult, "Duplicates of 'User Role Pairs' are not allowed", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Can_Update_Roles_To_Remove_Them()
        {
            //Arrange
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites();

            var newRight = CreateRight(createdRegistration, A<Guid>(), A<int>(), A<Guid>(), A<int>());
            var newUserRolePair = CreateUserRolePair(newRight.Role.Uuid, newRight.User.Uuid);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(newRight.User.Uuid, newRight.UserId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(newRight.Role.Uuid, newRight.RoleId);

            ExpectRoleAssignmentReturns(createdRegistration, newRight.RoleId, newRight.UserId, newRight);

            var rightToRemove = CreateRight(createdRegistration, A<Guid>(), A<int>(), A<Guid>(), A<int>());

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(rightToRemove.User.Uuid, rightToRemove.UserId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(rightToRemove.Role.Uuid, rightToRemove.RoleId);

            var rightToKeep = CreateRight(createdRegistration, A<Guid>(), A<int>(), A<Guid>(), A<int>());
            var userRolePairToKeep = CreateUserRolePair(rightToKeep.Role.Uuid, rightToKeep.User.Uuid);
            createdRegistration.Rights.Add(rightToRemove);
            createdRegistration.Rights.Add(rightToKeep);

            ExpectRoleRemovalReturns(createdRegistration, rightToRemove.RoleId, rightToRemove.UserId, rightToRemove);
            ExpectGetDataProcessingRegistrationReturns(createdRegistration.Uuid, createdRegistration);

            var roles = CreateUpdatedDataProcessingRegistrationRoles(new List<UserRolePair>()
            {
                newUserRolePair, userRolePairToKeep
            });

            var updateParameters = new DataProcessingRegistrationModificationParameters
            {
                Roles = roles
            };

            //Act
            var updateResult = _sut.Update(createdRegistration.Uuid, updateParameters);

            //Assert
            Assert.True(updateResult.Ok);
            AssertTransactionCommitted(transaction);
            _dprServiceMock.Verify(x => x.AssignRole(createdRegistration.Id, newRight.RoleId, newRight.UserId), Times.Once);
            _dprServiceMock.Verify(x => x.RemoveRole(createdRegistration.Id, rightToRemove.RoleId, rightToRemove.UserId), Times.Once);

            _dprServiceMock.Verify(x => x.AssignRole(createdRegistration.Id, rightToKeep.RoleId, rightToKeep.UserId), Times.Never);
            _dprServiceMock.Verify(x => x.RemoveRole(createdRegistration.Id, rightToKeep.RoleId, rightToKeep.UserId), Times.Never);
        }

        [Fact]
        public void Can_Create_With_ExternalReferences()
        {
            //Arrange
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(externalReferences: externalReferences);

            ExpectBatchUpdateExternalReferencesReturns(createdRegistration, externalReferences, Maybe<OperationError>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_With_ExternalReferences_If_BatchUpdate_Fails()
        {
            //Arrange
            var externalReferences = Many<UpdatedExternalReferenceProperties>().ToList();
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(externalReferences: externalReferences);

            var operationError = A<OperationError>();

            ExpectBatchUpdateExternalReferencesReturns(createdRegistration, externalReferences, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Failed to update references with error message", operationError.FailureType, transaction);
        }

        [Fact]
        public void Can_Add_Role()
        {
            //Arrange
            var existingAssignment = CreateUpdatedDataProcessingRegistrationRoles(A<UserRolePair>().WrapAsEnumerable());
            var (_, _, dpr, transaction) = SetupCreateScenarioPrerequisites(roles: existingAssignment);
            var newRight = CreateRight(dpr, A<Guid>(), A<int>(), A<Guid>(), A<int>());
            var newAssignment = CreateUserRolePair(newRight.Role.Uuid, newRight.User.Uuid);

            ExpectGetDataProcessingRegistrationReturns(dpr.Uuid, dpr);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(newAssignment.UserUuid, newRight.UserId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(newAssignment.RoleUuid, newRight.RoleId);

            ExpectRoleAssignmentReturns(dpr, newRight.RoleId, newRight.UserId, newRight);

            //Act
            var createResult = _sut.AddRole(dpr.Uuid, newAssignment);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Remove_Role()
        {
            //Arrange
            var userRolePairToRemove = A<UserRolePair>();
            var userId = A<int>();
            var roleId = A<int>();
            var existingAssignment = CreateUpdatedDataProcessingRegistrationRoles(new List<UserRolePair>
                { userRolePairToRemove, A<UserRolePair>() });
            var (_, _, dpr, transaction) = SetupCreateScenarioPrerequisites(roles: existingAssignment);
            var rightToRemove = CreateRight(dpr, userRolePairToRemove.RoleUuid, roleId, userRolePairToRemove.UserUuid, userId);
            dpr.Rights = new List<DataProcessingRegistrationRight> { rightToRemove };

            ExpectGetDataProcessingRegistrationReturns(dpr.Uuid, dpr);

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<User>(userRolePairToRemove.UserUuid, userId);
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingRegistrationRole>(userRolePairToRemove.RoleUuid, roleId);

            ExpectRoleRemovalReturns(dpr, roleId, userId, rightToRemove);

            //Act
            var createResult = _sut.RemoveRole(dpr.Uuid, userRolePairToRemove);

            //Assert
            Assert.True(createResult.Ok);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Create_With_Responsible_Unit()
        {
            Guid? orgUnitUuid = A<Guid>();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() };
            var (orgUuid, parameters, dpr, _) = SetupCreateScenarioPrerequisites(generalData: generalData);
            dpr.Organization = new Organization { OrgUnits = { new OrganizationUnit { Uuid = orgUnitUuid.Value } } };

            var result = _sut.Create(orgUuid, parameters);

            Assert.True(result.Ok);
            Assert.Equal(result.Value.ResponsibleOrganizationUnit.Uuid, orgUnitUuid);
        }

        [Fact]
        public void Can_Update_With_Responsible_Unit()
        {
            //Arrange
            Guid? orgUnitUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                General = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() }
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Organization = new Organization { OrgUnits = { new OrganizationUnit { Uuid = orgUnitUuid.Value } } }
            };
            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();

            ExpectGetDataProcessingRegistrationReturns(dprUuid, dataProcessingRegistration);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(result.Value.ResponsibleOrganizationUnit.Uuid, orgUnitUuid);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Reset_Responsible_Unit()
        {
            //Arrange
            Guid? orgUnitUuid = null;
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                General = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() }
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                ResponsibleOrganizationUnit = new OrganizationUnit { Uuid = A<Guid>() }
            };
            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();

            ExpectGetDataProcessingRegistrationReturns(dprUuid, dataProcessingRegistration);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Null(result.Value.ResponsibleOrganizationUnit);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Can_Not_Update_If_Unit_Does_Not_Exist_In_Organization()
        {
            //Arrange
            Guid? orgUnitUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                General = new UpdatedDataProcessingRegistrationGeneralDataParameters { ResponsibleUnitUuid = orgUnitUuid.AsChangedValue() }
            };
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Organization = new Organization()
            };
            var transaction = ExpectTransaction();
            var dprUuid = A<Guid>();

            ExpectGetDataProcessingRegistrationReturns(dprUuid, dataProcessingRegistration);

            //Act
            var result = _sut.Update(dprUuid, parameters);

            //Assert
            Assert.True(result.Failed);
            AssertTransactionNotCommitted(transaction);
        }

        private void ExpectBatchUpdateExternalReferencesReturns(DataProcessingRegistration dpr, IEnumerable<UpdatedExternalReferenceProperties> externalReferences, Maybe<OperationError> value)
        {
            _referenceServiceMock
                .Setup(x => x.UpdateExternalReferences(ReferenceRootType.DataProcessingRegistration, dpr.Id, externalReferences))
                .Returns(value);
        }

        private void ExpectRoleAssignmentReturns(DataProcessingRegistration dpr, int roleId, int userId, Result<DataProcessingRegistrationRight, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.AssignRole(dpr.Id, roleId, userId)).Returns(result);
        }

        private void ExpectRoleRemovalReturns(DataProcessingRegistration dpr, int roleId, int userId, Result<DataProcessingRegistrationRight, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.RemoveRole(dpr.Id, roleId, userId)).Returns(result);
        }

        private UpdatedDataProcessingRegistrationRoles CreateUpdatedDataProcessingRegistrationRoles(IEnumerable<UserRolePair> roles)
        {
            return new UpdatedDataProcessingRegistrationRoles
            {
                UserRolePairs = roles.FromNullable().AsChangedValue()
            };
        }

        private (Guid organizationUuid, DataProcessingRegistrationModificationParameters parameters, DataProcessingRegistration createdRegistration, Mock<IDatabaseTransaction> transaction) SetupCreateScenarioPrerequisites(
            UpdatedDataProcessingRegistrationGeneralDataParameters generalData = null,
            IEnumerable<Guid> systemUsageUuids = null,
            UpdatedDataProcessingRegistrationOversightDataParameters oversightData = null,
            UpdatedDataProcessingRegistrationRoles roles = null,
            IEnumerable<UpdatedExternalReferenceProperties> externalReferences = null)
        {
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue(),
                General = generalData.FromNullable(),
                SystemUsageUuids = systemUsageUuids.FromNullable(),
                Oversight = oversightData.FromNullable(),
                Roles = roles.FromNullable(),
                ExternalReferences = externalReferences.FromNullable()
            };
            var createdRegistration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Uuid = A<Guid>(),
                AssociatedContracts = new List<ItContract> { new() { Id = A<int>() } }
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);
            ExpectCreateDataProcessingRegistrationReturns(orgDbId, parameters, parameters.Name.NewValue, createdRegistration);
            return (organizationUuid, parameters, createdRegistration, transaction);
        }

        private static UserRolePair CreateUserRolePair(Guid roleUuid, Guid userUuid)
        {
            return new UserRolePair(userUuid, roleUuid);
        }

        private static DataProcessingRegistrationRight CreateRight(DataProcessingRegistration dpr, Guid roleUuid, int roleId, Guid userUuid, int userId)
        {
            return new DataProcessingRegistrationRight()
            {
                Object = dpr,
                Role = new DataProcessingRegistrationRole()
                {
                    Id = roleId,
                    Uuid = roleUuid
                },
                RoleId = roleId,
                User = new User()
                {
                    Id = userId,
                    Uuid = userUuid
                },
                UserId = userId
            };
        }

        private void ExpectUpdateNameReturns(int dprId, string nameNewValue, Result<DataProcessingRegistration, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.UpdateName(dprId, nameNewValue)).Returns(result);
        }

        private void ExpectGetDataProcessingRegistrationReturns(Guid dprUuid, Result<DataProcessingRegistration, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.GetByUuid(dprUuid)).Returns(result);
        }

        private static void AssertFailureWithKnownError(Result<DataProcessingRegistration, OperationError> result, OperationError operationError, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Equal(operationError, result.Error);
            AssertTransactionNotCommitted(transaction);
        }

        private void AssertFailureWithKnownErrorDetails(Result<DataProcessingRegistration, OperationError> result, string errorMessageContent, OperationFailure failure, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Contains(errorMessageContent, result.Error.Message.GetValueOrEmptyString());
            Assert.Equal(failure, result.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
        }

        private void ExpectCreateDataProcessingRegistrationReturns(int orgDbId, DataProcessingRegistrationModificationParameters parameters, string name, Result<DataProcessingRegistration, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.Create(orgDbId, name)).Returns(result);
        }

        private void AssertTransactionCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
            transactionMock.Verify(x => x.Commit());
        }
        private static void AssertTransactionNotCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid? uuid, Maybe<int> dbId) where T : class, IHasUuid, IHasId
        {
            if (uuid.HasValue)
                _identityResolverMock.Setup(x => x.ResolveDbId<T>(uuid.Value)).Returns(dbId);
        }

        private void ExpectUpdateMultiAssignmentReturns<TAssignmentInput, TAssignmentState>(DataProcessingRegistration registration, Maybe<IEnumerable<Guid>> assignmentUuids, Maybe<OperationError> result)
            where TAssignmentState : class, IHasId, IHasUuid
        {
            _assignmentUpdateServiceMock
                .Setup(x => x.UpdateUniqueMultiAssignment(
                    It.IsAny<string>(),
                    registration,
                    It.Is<Maybe<IEnumerable<Guid>>>(uuids =>
                        uuids.HasValue == assignmentUuids.HasValue &&
                        (uuids.IsNone || uuids.Value.SequenceEqual(assignmentUuids.Value))),
                    It.IsAny<Func<Guid, Result<TAssignmentInput, OperationError>>>(),
                    It.IsAny<Func<DataProcessingRegistration, IEnumerable<TAssignmentState>>>(),
                    It.IsAny<Func<DataProcessingRegistration, TAssignmentInput, Maybe<OperationError>>>(),
                    It.IsAny<Func<DataProcessingRegistration, TAssignmentState, Maybe<OperationError>>>(),
                    It.IsAny<Func<DataProcessingRegistration, TAssignmentState, Maybe<OperationError>>>()))
                .Returns(result);
        }

        private void ExpectUpdateMainContractReturns(int dprId, int contractId,
            Result<DataProcessingRegistration, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.UpdateMainContract(dprId, contractId)).Returns(result);
        }

        private void ExpectRemoveMainContractReturns(int dprId,
            Result<DataProcessingRegistration, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.RemoveMainContract(dprId)).Returns(result);
        }
    }
}
