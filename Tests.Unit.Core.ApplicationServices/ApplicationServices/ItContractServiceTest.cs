﻿using System.Collections.Generic;
using System.Data;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class ItContractServiceTest : WithAutoFixture
    {
        private readonly ItContractService _sut;
        private readonly Mock<IGenericRepository<ItContract>> _contractRepository;
        private readonly Mock<IGenericRepository<EconomyStream>> _economyStreamRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        public ItContractServiceTest()
        {
            _contractRepository = new Mock<IGenericRepository<ItContract>>();
            _economyStreamRepository = new Mock<IGenericRepository<EconomyStream>>();
            _transactionManager = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new ItContractService(
                _contractRepository.Object,
                _economyStreamRepository.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _authorizationContext.Object);
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var contractId = A<int>();
            ExpectGetContractReturns(contractId, default(ItContract));

            //Act
            var result = _sut.Delete(contractId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var contractId = A<int>();
            var itContract = new ItContract();
            ExpectGetContractReturns(contractId, itContract);
            ExpectAllowDeleteReturns(itContract, false);

            //Act
            var result = _sut.Delete(contractId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Delete_Deletes_EconomyStreams_And_Raises_Domain_Event_And_Returns_Ok()
        {
            //Arrange
            var contractId = A<int>();
            var externEconomyStream1 = CreateEconomyStream();
            var externEconomyStream2 = CreateEconomyStream();
            var internEconomyStream1 = CreateEconomyStream();
            var internEconomyStream2 = CreateEconomyStream();
            var itContract = new ItContract
            {
                ExternEconomyStreams = new List<EconomyStream> { externEconomyStream1, externEconomyStream2 },
                InternEconomyStreams = new List<EconomyStream> { internEconomyStream1, internEconomyStream2 }
            };
            var transaction = new Mock<IDatabaseTransaction>();
            ExpectGetContractReturns(contractId, itContract);
            ExpectAllowDeleteReturns(itContract, true);
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.Delete(contractId);

            //Assert
            Assert.True(result.Ok);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(It.IsAny<EconomyStream>()), Times.Exactly(4));
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(externEconomyStream1), Times.Once);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(externEconomyStream2), Times.Once);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(internEconomyStream1), Times.Once);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(internEconomyStream2), Times.Once);
            _contractRepository.Verify(x => x.DeleteWithReferencePreload(itContract), Times.Once);
            _domainEvents.Verify(x => x.Raise(It.Is<ContractDeleted>(cd => cd.DeletedContract == itContract)), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        private EconomyStream CreateEconomyStream()
        {
            return new EconomyStream { Id = A<int>() };
        }

        private void ExpectAllowDeleteReturns(ItContract itContract, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(itContract)).Returns(value);
        }

        private void ExpectGetContractReturns(int contractId, ItContract itContract)
        {
            _contractRepository.Setup(x => x.GetByKey(contractId)).Returns(itContract);
        }
    }
}