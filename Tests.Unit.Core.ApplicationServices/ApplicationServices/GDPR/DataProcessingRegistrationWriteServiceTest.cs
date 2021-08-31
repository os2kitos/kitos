using System;
using System.Data;
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

        //TODO: From here
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

        //TODO: Update success scenarios
        //TODO: Update edge cases

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
            _transactionManagerMock.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(trasactionMock.Object);
            return trasactionMock;
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid? uuid, Maybe<int> dbId) where T : class, IHasUuid, IHasId
        {
            if (uuid.HasValue)
                _identityResolverMock.Setup(x => x.ResolveDbId<T>(uuid.Value)).Returns(dbId);
        }
    }
}
