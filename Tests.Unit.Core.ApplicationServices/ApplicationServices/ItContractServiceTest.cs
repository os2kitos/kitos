using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Contract;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class ItContractServiceTest : WithAutoFixture
    {
        private readonly ItContractService _sut;
        private readonly Mock<IItContractRepository> _contractRepository;
        private readonly Mock<IGenericRepository<EconomyStream>> _economyStreamRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IReferenceService> _referenceService;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IContractDataProcessingRegistrationAssignmentService> _contractDataProcessingRegistrationAssignmentService;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;

        public ItContractServiceTest()
        {
            _contractRepository = new Mock<IItContractRepository>();
            _economyStreamRepository = new Mock<IGenericRepository<EconomyStream>>();
            _transactionManager = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _referenceService = new Mock<IReferenceService>();
            _logger = new Mock<ILogger>();
            _contractDataProcessingRegistrationAssignmentService = new Mock<IContractDataProcessingRegistrationAssignmentService>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new ItContractService(
                _contractRepository.Object,
                _economyStreamRepository.Object,
                _referenceService.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _authorizationContext.Object,
                _logger.Object,
                _contractDataProcessingRegistrationAssignmentService.Object,
                _userContextMock.Object);
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
            _referenceService.Setup(x => x.DeleteByContractId(contractId)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new List<ExternalReference>()));

            //Act
            var result = _sut.Delete(contractId);

            //Assert
            Assert.True(result.Ok);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(It.IsAny<EconomyStream>()), Times.Exactly(4));
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(externEconomyStream1), Times.Once);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(externEconomyStream2), Times.Once);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(internEconomyStream1), Times.Once);
            _economyStreamRepository.Verify(x => x.DeleteWithReferencePreload(internEconomyStream2), Times.Once);
            _contractRepository.Verify(x => x.DeleteContract(itContract), Times.Once);
            _domainEvents.Verify(x => x.Raise(It.Is<ContractDeleted>(cd => cd.DeletedContract == itContract)), Times.Once);
            _referenceService.Verify(x => x.DeleteByContractId(contractId), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Can_GetDataProcessingRegistrationsWhichCanBeAssigned()
        {
            //Arrange
            var id = A<int>();
            var dataProcessingRegistration1Id = A<int>();
            var dataProcessingRegistration2Id = dataProcessingRegistration1Id + 1;
            var nameQuery = A<string>();
            var contract = new ItContract();
            ExpectGetContractReturns(id, contract);
            ExpectAllowReadReturns(contract, true);
            var dataProcessingRegistrations = new[] { new DataProcessingRegistration() { Id = dataProcessingRegistration1Id,  Name = $"{nameQuery}{1}" }, new DataProcessingRegistration() { Id = dataProcessingRegistration2Id, Name = $"{nameQuery}{1}" } };
            _contractDataProcessingRegistrationAssignmentService.Setup(x => x.GetApplicableDataProcessingRegistrations(contract)).Returns(dataProcessingRegistrations.AsQueryable());

            //Act
            var result = _sut.GetDataProcessingRegistrationsWhichCanBeAssigned(id, nameQuery, new Random().Next(2, 100));

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(dataProcessingRegistrations, result.Value);
        }

        [Fact]
        public void Cannot_GetDataProcessingRegistrationsWhichCanBeAssigned_If_Contract_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Contract_NotFound(id => _sut.GetDataProcessingRegistrationsWhichCanBeAssigned(id, A<string>(), new Random().Next(2, 100)));
        }

        [Fact]
        public void Cannot_GetDataProcessingRegistrationsWhichCanBeAssigned_If_Read_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Contract_Insufficient_ReadAccess(id => _sut.GetDataProcessingRegistrationsWhichCanBeAssigned(id, A<string>(), new Random().Next(2, 100)));
        }

        [Fact]
        public void Can_AssignDataProcessingRegistration()
        {
            //Arrange
            var id = A<int>();
            var contract = new ItContract();
            var dataProcessingRegistrationId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration();
            ExpectGetContractReturns(id, contract);
            ExpectAllowModifyReturns(contract, true);
            _contractDataProcessingRegistrationAssignmentService.Setup(x => x.AssignDataProcessingRegistration(contract, dataProcessingRegistrationId)).Returns(dataProcessingRegistration);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.AssignDataProcessingRegistration(id, dataProcessingRegistrationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_AssignDataProcessingRegistration_If_Contract_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Contract_NotFound(id => _sut.AssignDataProcessingRegistration(id, A<int>()));
        }

        [Fact]
        public void Cannot_AssignDataProcessingRegistration_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Contract_Insufficient_WriteAccess(id => _sut.AssignDataProcessingRegistration(id, A<int>()));
        }

        [Fact]
        public void Can_RemoveDataProcessingRegistration()
        {
            //Arrange
            var id = A<int>();
            var contract = new ItContract();
            var dataProcessingRegistrationId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration();
            ExpectGetContractReturns(id, contract);
            ExpectAllowModifyReturns(contract, true);
            _contractDataProcessingRegistrationAssignmentService.Setup(x => x.RemoveDataProcessingRegistration(contract, dataProcessingRegistrationId)).Returns(dataProcessingRegistration);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            var result = _sut.RemoveDataProcessingRegistration(id, dataProcessingRegistrationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration, result.Value);
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void Cannot_RemoveDataProcessingRegistration_If_Contract_Is_Not_Found()
        {
            Test_Command_Which_Fails_With_Contract_NotFound(id => _sut.RemoveDataProcessingRegistration(id, A<int>()));
        }

        [Fact]
        public void Cannot_RemoveDataProcessingRegistration_If_Write_Access_Is_Denied()
        {
            Test_Command_Which_Fails_With_Contract_Insufficient_WriteAccess(id => _sut.RemoveDataProcessingRegistration(id, A<int>()));
        }

        [Fact]
        public void GetProject_Returns_Project()
        {
            //Arrange
            var contractUuid = A<Guid>();
            var contract = new ItContract();

            _contractRepository.Setup(x => x.GetContract(contractUuid)).Returns(contract);
            _authorizationContext.Setup(x => x.AllowReads(contract)).Returns(true);

            //Act
            var contractResult = _sut.GetContract(contractUuid);

            //Assert
            Assert.True(contractResult.Ok);
            Assert.Same(contract, contractResult.Value);
        }

        [Fact]
        public void GetInterface_Returns_Forbidden_If_Not_Read_Access()
        {
            //Arrange
            var contractUuid = A<Guid>();
            var contract = new ItContract();

            _contractRepository.Setup(x => x.GetContract(contractUuid)).Returns(contract);
            _authorizationContext.Setup(x => x.AllowReads(contract)).Returns(false);

            //Act
            var contractResult = _sut.GetContract(contractUuid);

            //Assert
            Assert.True(contractResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, contractResult.Error.FailureType);
        }

        [Fact]
        public void GetInterface_Returns_NotFound_If_No_Interface()
        {
            //Arrange
            var contractUuid = A<Guid>();

            _contractRepository.Setup(x => x.GetContract(contractUuid)).Returns(Maybe<ItContract>.None);

            //Act
            var contractResult = _sut.GetContract(contractUuid);

            //Assert
            Assert.True(contractResult.Failed);
            Assert.Equal(OperationFailure.NotFound, contractResult.Error.FailureType);
        }

        [Fact]
        public void GetAvailableProjects_Returns_Projects()
        {
            //Arrange
            var contractUuid = A<Guid>();
            var contract = new ItContract()
            {
                Uuid = contractUuid
            };

            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.All);
            _contractRepository.Setup(x => x.GetContracts()).Returns(new List<ItContract>() { contract }.AsQueryable());

            //Act
            var result = _sut.GetAvailableContracts();

            //Assert
            var projectResult = Assert.Single(result.ToList());
            Assert.Equal(contractUuid, projectResult.Uuid);
        }

        private EconomyStream CreateEconomyStream()
        {
            return new EconomyStream { Id = A<int>() };
        }

        private void ExpectAllowDeleteReturns(ItContract itContract, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(itContract)).Returns(value);
        }

        private void ExpectAllowReadReturns(ItContract itContract, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(itContract)).Returns(value);
        }

        private void ExpectAllowModifyReturns(ItContract itContract, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(itContract)).Returns(value);
        }

        private void ExpectGetContractReturns(int contractId, ItContract itContract)
        {
            _contractRepository.Setup(x => x.GetById(contractId)).Returns(itContract);
        }

        /// <summary>
        /// Helper test to make it easy to cover the "Missing Write access" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Contract_Insufficient_WriteAccess<TSuccess>(Func<int, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            var contract = new ItContract();
            ExpectGetContractReturns(id, contract);
            ExpectAllowModifyReturns(contract, false);

            //Act
            var result = command(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        /// <summary>
        /// Helper test to make it easy to cover the "Missing read access" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Contract_Insufficient_ReadAccess<TSuccess>(Func<int, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            var contract = new ItContract();
            ExpectGetContractReturns(id, contract);
            ExpectAllowReadReturns(contract, false);

            //Act
            var result = command(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }


        /// <summary>
        /// Helper test to make it easy to cover the "Contract not found" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Contract_NotFound<TSuccess>(Func<int, Result<TSuccess, OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            ExpectGetContractReturns(id, null);

            //Act
            var result = command(id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }
    }
}
