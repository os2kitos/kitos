﻿using System;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Moq;
using Moq.Language.Flow;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Contract
{
    public class ItContractWriteServiceTest : WithAutoFixture
    {
        private readonly ItContractWriteService _sut;
        private readonly Mock<IItContractService> _itContractServiceMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IOptionResolver> _optionResolverMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;

        public ItContractWriteServiceTest()
        {
            _itContractServiceMock = new Mock<IItContractService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _optionResolverMock = new Mock<IOptionResolver>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _sut = new ItContractWriteService(_itContractServiceMock.Object, _identityResolverMock.Object, _optionResolverMock.Object, _transactionManagerMock.Object, _domainEventsMock.Object, _databaseControlMock.Object);
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var contractId = A<Guid>();
            var contractDbId = A<int>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(contractId, contractDbId);
            _itContractServiceMock.Setup(x => x.Delete(contractDbId)).Returns(new ItContract());

            //Act
            var error = _sut.Delete(contractId);

            //Assert
            Assert.True(error.IsNone);
        }

        [Fact]
        public void Cannot_Delete_If_Delete_Fails_In_Service()
        {
            //Arrange
            var contractId = A<Guid>();
            var contractDbId = A<int>();
            var operationFailure = A<OperationFailure>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(contractId, contractDbId);
            _itContractServiceMock.Setup(x => x.Delete(contractDbId)).Returns(operationFailure);

            //Act
            var error = _sut.Delete(contractId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(operationFailure, error.Value.FailureType);
            Assert.Equal("Failed deleting contract", error.Value.Message.Value);
        }

        [Fact]
        public void Cannot_Delete_If_DbId_Resolution_Fails()
        {
            //Arrange
            var contractId = A<Guid>();

            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<ItContract>(contractId, Maybe<int>.None);

            //Act
            var error = _sut.Delete(contractId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
            Assert.Equal("Invalid contract uuid", error.Value.Message.Value);
        }

        [Fact]
        public void Can_Create_With_Name()
        {
            //Arrange
            var name = A<string>();
            var parameters = new ItContractModificationParameters() { Name = name.AsChangedValue() };
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();
            var itContract = new ItContract() { Id = A<int>(), Uuid = A<Guid>() };
            var organizationId = A<int>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, organizationId);
            ExpectCreateReturns(organizationId, name, itContract);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itContract, result.Value);
            AssertTransactionCommitted(transaction);
        }

        [Fact]
        public void Cannot_Create_If_Creation_Fails()
        {
            //Arrange
            var name = A<string>();
            var parameters = new ItContractModificationParameters() { Name = name.AsChangedValue() };
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();
            var organizationId = A<int>();
            var operationError = A<OperationError>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, organizationId);
            ExpectCreateReturns(organizationId, name, operationError);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownError(result, operationError, transaction);
        }

        [Fact]
        public void Cannot_Create_If_OrganizationId_Resolution_Fails()
        {
            //Arrange
            var name = A<string>();
            var parameters = new ItContractModificationParameters() { Name = name.AsChangedValue() };
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();
            ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<Organization>(organizationUuid, Maybe<int>.None);

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Organization id not valid", OperationFailure.BadInput, transaction);
        }

        [Fact]
        public void Cannot_Create_Without_Defined_Name()
        {
            //Arrange
            var parameters = new ItContractModificationParameters();
            var transaction = ExpectTransaction();
            var organizationUuid = A<Guid>();

            //Act
            var result = _sut.Create(organizationUuid, parameters);

            //Assert
            AssertFailureWithKnownErrorDetails(result, "Name must be provided", OperationFailure.BadInput, transaction);
        }

        private void ExpectCreateReturns(int organizationId, string name, Result<ItContract, OperationError> result)
        {
            _itContractServiceMock.Setup(x => x.Create(organizationId, name)).Returns(result);
        }

        private void ExpectIfUuidHasValueResolveIdentityDbIdReturnsId<T>(Guid? uuid, Maybe<int> dbId) where T : class, IHasUuid, IHasId
        {
            if (uuid.HasValue)
                _identityResolverMock.Setup(x => x.ResolveDbId<T>(uuid.Value)).Returns(dbId);
        }

        private void AssertFailureWithKnownError(Result<ItContract, OperationError> result, OperationError operationError, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Equal(operationError, result.Error);
            AssertTransactionNotCommitted(transaction);
        }

        private void AssertFailureWithKnownErrorDetails(Result<ItContract, OperationError> result, string errorMessageContent, OperationFailure failure, Mock<IDatabaseTransaction> transaction)
        {
            Assert.True(result.Failed);
            Assert.Contains(errorMessageContent, result.Error.Message.GetValueOrEmptyString());
            Assert.Equal(failure, result.Error.FailureType);
            AssertTransactionNotCommitted(transaction);
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
            var trasactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(trasactionMock.Object);
            return trasactionMock;
        }
    }
}
