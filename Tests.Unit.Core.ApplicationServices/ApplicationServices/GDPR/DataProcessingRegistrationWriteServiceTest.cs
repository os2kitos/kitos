using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.GDPR.Write;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
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
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;

        public DataProcessingRegistrationWriteServiceTest()
        {
            _dprServiceMock = new Mock<IDataProcessingRegistrationApplicationService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _sut = new DataProcessingRegistrationWriteService(
                _dprServiceMock.Object,
                _identityResolverMock.Object,
                Mock.Of<ILogger>(),
                _domainEventsMock.Object,
                _transactionManagerMock.Object,
                _databaseControlMock.Object);
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

        [Fact]
        public void Can_CreateWith_GeneralData_InsecureCountriesSubjectToDataTransfer()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                InsecureCountriesSubjectToDataTransferUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Make sure we have som existing countries and add one which is shared with the new state. That one, we don't expect to be removed
            var existingCountryAssigmentIds = Many<Guid>().Append(inputUuids.RandomItem()).ToList();
            var optionMap = inputUuids
                .Concat(existingCountryAssigmentIds)
                .Distinct()
                .ToDictionary(uuid => uuid, uuid => new DataProcessingCountryOption { Uuid = uuid, Id = A<int>() });

            createdRegistration.InsecureCountriesSubjectToDataTransfer = existingCountryAssigmentIds.Select(uuid => optionMap[uuid]).ToList();
            inputUuids.ForEach(uuid => ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<DataProcessingCountryOption>(uuid, optionMap[uuid].Id));

            var expectedRemovals = existingCountryAssigmentIds.Except(inputUuids).ToList();
            var expectedAdditions = inputUuids.Except(existingCountryAssigmentIds).ToList();
            foreach (var expectedRemoval in expectedRemovals) 
                _dprServiceMock.Setup(x=>x.RemoveInsecureThirdCountry(createdRegistration.Id,optionMap[expectedRemoval].Id)).Returns(optionMap[expectedRemoval]);
            
            foreach (var expectedAddition in expectedAdditions)
                _dprServiceMock.Setup(x => x.AssignInsecureThirdCountry(createdRegistration.Id, optionMap[expectedAddition].Id)).Returns(optionMap[expectedAddition]);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);

            foreach (var expectedRemoval in expectedRemovals)
            {
                _dprServiceMock.Verify(x => x.RemoveInsecureThirdCountry(createdRegistration.Id, optionMap[expectedRemoval].Id), Times.Once);
                _dprServiceMock.Verify(x => x.AssignInsecureThirdCountry(createdRegistration.Id, optionMap[expectedRemoval].Id), Times.Never);
            }

            foreach (var expectedAddition in expectedAdditions)
            {
                _dprServiceMock.Verify(x => x.AssignInsecureThirdCountry(createdRegistration.Id, optionMap[expectedAddition].Id),Times.Once);
                _dprServiceMock.Verify(x => x.RemoveInsecureThirdCountry(createdRegistration.Id, optionMap[expectedAddition].Id),Times.Never);
            }
        }

        [Fact]
        public void Can_CreateWith_GeneralData_DataProcessor()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataProcessorUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Make sure we have som existing organizations and add one which is shared with the new state. That one, we don't expect to be removed
            var assigmentIds = Many<Guid>().Append(inputUuids.RandomItem()).ToList();
            var entityMap = inputUuids
                .Concat(assigmentIds)
                .Distinct()
                .ToDictionary(uuid => uuid, uuid => new Organization() { Uuid = uuid, Id = A<int>() });

            createdRegistration.DataProcessors = assigmentIds.Select(uuid => entityMap[uuid]).ToList();
            inputUuids.ForEach(uuid => ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(uuid, entityMap[uuid].Id));

            var expectedRemovals = assigmentIds.Except(inputUuids).ToList();
            var expectedAdditions = inputUuids.Except(assigmentIds).ToList();
            foreach (var expectedRemoval in expectedRemovals)
                _dprServiceMock.Setup(x => x.RemoveDataProcessor(createdRegistration.Id, entityMap[expectedRemoval].Id)).Returns(entityMap[expectedRemoval]);

            foreach (var expectedAddition in expectedAdditions)
                _dprServiceMock.Setup(x => x.AssignDataProcessor(createdRegistration.Id, entityMap[expectedAddition].Id)).Returns(entityMap[expectedAddition]);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);

            foreach (var expectedRemoval in expectedRemovals)
            {
                _dprServiceMock.Verify(x => x.RemoveDataProcessor(createdRegistration.Id, entityMap[expectedRemoval].Id), Times.Once);
                _dprServiceMock.Verify(x => x.AssignDataProcessor(createdRegistration.Id, entityMap[expectedRemoval].Id), Times.Never);
            }

            foreach (var expectedAddition in expectedAdditions)
            {
                _dprServiceMock.Verify(x => x.AssignDataProcessor(createdRegistration.Id, entityMap[expectedAddition].Id), Times.Once);
                _dprServiceMock.Verify(x => x.RemoveDataProcessor(createdRegistration.Id, entityMap[expectedAddition].Id), Times.Never);
            }
        }

        [Fact]
        public void Can_CreateWith_GeneralData_SubDataProcessor()
        {
            //Arrange
            var inputUuids = Many<Guid>().ToList();
            var generalData = new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                SubDataProcessorUuids = inputUuids.FromNullable<IEnumerable<Guid>>().AsChangedValue()
            };
            var (organizationUuid, parameters, createdRegistration, transaction) = SetupCreateScenarioPrerequisites(generalData: generalData);

            //Make sure we have som existing organizations and add one which is shared with the new state. That one, we don't expect to be removed
            var assigmentIds = Many<Guid>().Append(inputUuids.RandomItem()).ToList();
            var entityMap = inputUuids
                .Concat(assigmentIds)
                .Distinct()
                .ToDictionary(uuid => uuid, uuid => new Organization { Uuid = uuid, Id = A<int>() });

            createdRegistration.SubDataProcessors = assigmentIds.Select(uuid => entityMap[uuid]).ToList();
            inputUuids.ForEach(uuid => ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(uuid, entityMap[uuid].Id));

            var expectedRemovals = assigmentIds.Except(inputUuids).ToList();
            var expectedAdditions = inputUuids.Except(assigmentIds).ToList();
            foreach (var expectedRemoval in expectedRemovals)
                _dprServiceMock.Setup(x => x.RemoveSubDataProcessor(createdRegistration.Id, entityMap[expectedRemoval].Id)).Returns(entityMap[expectedRemoval]);

            foreach (var expectedAddition in expectedAdditions)
                _dprServiceMock.Setup(x => x.AssignSubDataProcessor(createdRegistration.Id, entityMap[expectedAddition].Id)).Returns(entityMap[expectedAddition]);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(createdRegistration, result.Value);
            AssertTransactionCommitted(transaction);

            foreach (var expectedRemoval in expectedRemovals)
            {
                _dprServiceMock.Verify(x => x.RemoveSubDataProcessor(createdRegistration.Id, entityMap[expectedRemoval].Id), Times.Once);
                _dprServiceMock.Verify(x => x.AssignSubDataProcessor(createdRegistration.Id, entityMap[expectedRemoval].Id), Times.Never);
            }

            foreach (var expectedAddition in expectedAdditions)
            {
                _dprServiceMock.Verify(x => x.AssignSubDataProcessor(createdRegistration.Id, entityMap[expectedAddition].Id), Times.Once);
                _dprServiceMock.Verify(x => x.RemoveSubDataProcessor(createdRegistration.Id, entityMap[expectedAddition].Id), Times.Never);
            }
        }

        private (Guid organizationUuid, DataProcessingRegistrationModificationParameters parameters, DataProcessingRegistration createdRegistration, Mock<IDatabaseTransaction> transaction) SetupCreateScenarioPrerequisites(
            UpdatedDataProcessingRegistrationGeneralDataParameters generalData = null)
        {
            var organizationUuid = A<Guid>();
            var parameters = new DataProcessingRegistrationModificationParameters
            {
                Name = A<string>().AsChangedValue(),
                General = generalData.FromNullable()
            };
            var createdRegistration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var transaction = ExpectTransaction();
            var orgDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, orgDbId);
            ExpectCreateDataProcessingRegistrationReturns(orgDbId, parameters, parameters.Name.NewValue, createdRegistration);
            return (organizationUuid, parameters, createdRegistration, transaction);
        }

        private void ExpectUpdateNameReturns(int dprId, string nameNewValue, Result<DataProcessingRegistration, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.UpdateName(dprId, nameNewValue)).Returns(result);
        }

        private void ExpectGetDataProcessingRegistrationReturns(Guid dprUuid, Result<DataProcessingRegistration, OperationError> result)
        {
            _dprServiceMock.Setup(x => x.GetByUuid(dprUuid)).Returns(result);
        }

        private void AssertFailureWithKnownError(Result<DataProcessingRegistration, OperationError> result, OperationError operationError, Mock<IDatabaseTransaction> transaction)
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
        private void AssertTransactionNotCommitted(Mock<IDatabaseTransaction> transactionMock)
        {
            transactionMock.Verify(x => x.Commit(), Times.Never);
        }

        private Mock<IDatabaseTransaction> ExpectTransaction()
        {
            var trasactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(trasactionMock.Object);
            return trasactionMock;
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid? uuid, Maybe<int> dbId) where T : class, IHasUuid, IHasId
        {
            if (uuid.HasValue)
                _identityResolverMock.Setup(x => x.ResolveDbId<T>(uuid.Value)).Returns(dbId);
        }
    }
}
