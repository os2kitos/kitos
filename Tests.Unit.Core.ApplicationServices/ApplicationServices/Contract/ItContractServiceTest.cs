using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Contract;
using Core.DomainServices.Options;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Contract
{
    public class ItContractServiceTest : WithAutoFixture
    {
        private readonly ItContractService _sut;
        private readonly Mock<IItContractRepository> _contractRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IReferenceService> _referenceService;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IContractDataProcessingRegistrationAssignmentService> _contractDataProcessingRegistrationAssignmentService;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IOptionsService<ItContract, CriticalityType>> _criticalityOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, ItContractType>> _contractTypeOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, ItContractTemplateType>> _contractTemplateOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, PurchaseFormType>> _purchaseFromOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, ProcurementStrategyType>> _procurementStrategyOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, PaymentModelType>> _paymentModelOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, PaymentFreqencyType>> _paymentFreqencyOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, OptionExtendType>> _optionExtendOptionsServiceMock;
        private readonly Mock<IOptionsService<ItContract, TerminationDeadlineType>> _terminationDeadlineOptionsServiceMock;
        private readonly Mock<IGenericRepository<EconomyStream>> _economyStreamRepositoryMock;

        public ItContractServiceTest()
        {
            _contractRepository = new Mock<IItContractRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _referenceService = new Mock<IReferenceService>();
            _logger = new Mock<ILogger>();
            _contractDataProcessingRegistrationAssignmentService = new Mock<IContractDataProcessingRegistrationAssignmentService>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _criticalityOptionsServiceMock = new Mock<IOptionsService<ItContract, CriticalityType>>();
            _contractTypeOptionsServiceMock = new Mock<IOptionsService<ItContract, ItContractType>>();
            _contractTemplateOptionsServiceMock = new Mock<IOptionsService<ItContract, ItContractTemplateType>>();
            _purchaseFromOptionsServiceMock = new Mock<IOptionsService<ItContract, PurchaseFormType>>();
            _procurementStrategyOptionsServiceMock = new Mock<IOptionsService<ItContract, ProcurementStrategyType>>();
            _paymentModelOptionsServiceMock = new Mock<IOptionsService<ItContract, PaymentModelType>>();
            _paymentFreqencyOptionsServiceMock = new Mock<IOptionsService<ItContract, PaymentFreqencyType>>();
            _optionExtendOptionsServiceMock = new Mock<IOptionsService<ItContract, OptionExtendType>>();
            _terminationDeadlineOptionsServiceMock = new Mock<IOptionsService<ItContract, TerminationDeadlineType>>();
            _economyStreamRepositoryMock = new Mock<IGenericRepository<EconomyStream>>();

            _sut = new ItContractService(
                _contractRepository.Object,
                _referenceService.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _authorizationContext.Object,
                _logger.Object,
                _contractDataProcessingRegistrationAssignmentService.Object,
                _userContextMock.Object,
                _criticalityOptionsServiceMock.Object,
                _contractTypeOptionsServiceMock.Object,
                _contractTemplateOptionsServiceMock.Object,
                _purchaseFromOptionsServiceMock.Object,
                _procurementStrategyOptionsServiceMock.Object,
                _paymentModelOptionsServiceMock.Object,
                _paymentFreqencyOptionsServiceMock.Object,
                _optionExtendOptionsServiceMock.Object,
                _terminationDeadlineOptionsServiceMock.Object,
                _economyStreamRepositoryMock.Object);
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var contractId = A<int>();
            ExpectGetContractReturns(contractId, default);

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
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            _referenceService.Setup(x => x.DeleteByContractId(contractId)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new List<ExternalReference>()));

            //Act
            var result = _sut.Delete(contractId);

            //Assert
            Assert.True(result.Ok); 
            _economyStreamRepositoryMock.Verify(x => x.DeleteWithReferencePreload(It.IsAny<EconomyStream>()), Times.Exactly(4));
            _economyStreamRepositoryMock.Verify(x => x.DeleteWithReferencePreload(externEconomyStream1), Times.Once);
            _economyStreamRepositoryMock.Verify(x => x.DeleteWithReferencePreload(externEconomyStream2), Times.Once);
            _economyStreamRepositoryMock.Verify(x => x.DeleteWithReferencePreload(internEconomyStream1), Times.Once);
            _economyStreamRepositoryMock.Verify(x => x.DeleteWithReferencePreload(internEconomyStream2), Times.Once);
            _contractRepository.Verify(x => x.DeleteContract(itContract), Times.Once);
            _domainEvents.Verify(x => x.Raise(It.Is<EntityBeingDeletedEvent<ItContract>>(cd => cd.Entity == itContract)), Times.Once);
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
            var dataProcessingRegistrations = new[] { new DataProcessingRegistration() { Id = dataProcessingRegistration1Id, Name = $"{nameQuery}{1}" }, new DataProcessingRegistration() { Id = dataProcessingRegistration2Id, Name = $"{nameQuery}{1}" } };
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
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

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
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

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
        public void GetContract_Returns_Contract()
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
        public void GetContract_Returns_Forbidden_If_Not_Read_Access()
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
        public void GetContract_Returns_NotFound_If_No_Interface()
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
        public void Query_Returns_All_Contracts()
        {
            //Arrange
            var numberOfContracts = Math.Abs(A<int>());
            var registrations = CreateListOfContracts(numberOfContracts);
            _contractRepository.Setup(x => x.AsQueryable()).Returns(registrations.AsQueryable());
            ExpectCrossOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var contracts = _sut.Query();

            //Assert
            Assert.Equal(numberOfContracts, contracts.Count());
        }

        [Fact]
        public void Query_Returns_All_Contracts_From_Organizations_Where_User_Has_Access()
        {
            //Arrange
            var org1 = A<int>();
            var org2 = A<int>();
            var contract1 = new ItContract() { Id = A<int>(), OrganizationId = org1 };
            var contract2 = new ItContract() { Id = A<int>(), OrganizationId = org2 };

            var registrations = new List<ItContract>() { contract1, contract2, new ItContract { Id = A<int>() } };

            _contractRepository.Setup(x => x.AsQueryable()).Returns(registrations.AsQueryable());
            ExpectCrossOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.None);
            _userContextMock.Setup(x => x.OrganizationIds).Returns(new List<int>() { org1, org2 });

            //Act
            var contracts = _sut.Query();

            //Assert
            Assert.Equal(2, contracts.Count());
            var result1 = Assert.Single(contracts.Where(x => x.OrganizationId == org1));
            Assert.Same(contract1, result1);

            var result2 = Assert.Single(contracts.Where(x => x.OrganizationId == org2));
            Assert.Same(contract2, result2);
        }

        [Fact]
        public void Query_Returns_Contracts_Which_Satisfies_Conditions()
        {
            //Arrange
            var org1 = new Organization() { Id = A<int>(), Uuid = A<Guid>() };
            var org2 = new Organization() { Id = A<int>(), Uuid = A<Guid>() };

            var contract1 = new ItContract() { Id = A<int>(), Organization = org1 };
            var contract2 = new ItContract() { Id = A<int>(), Organization = org2 };

            var registrations = new List<ItContract>() { contract1, contract2, new ItContract { Id = A<int>() } }.AsQueryable();
            var queryMock = new Mock<IDomainQuery<ItContract>>();
            queryMock.Setup(x => x.Apply(registrations)).Returns(new List<ItContract>() { contract1 }.AsQueryable());
            var conditions = new List<IDomainQuery<ItContract>>() { queryMock.Object };

            _contractRepository.Setup(x => x.AsQueryable()).Returns(registrations);
            ExpectCrossOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var contracts = _sut.Query(conditions.ToArray());

            //Assert
            var result = Assert.Single(contracts);
            Assert.Same(contract1, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanCreateNewContractWithName_Returns_Ok(bool overlapFound)
        {
            //Arrange
            var name = A<string>();
            var organizationId = A<int>();
            var contract1 = new ItContract { Name = A<string>() };
            var contract2 = new ItContract { Name = overlapFound ? name : A<string>() };

            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(OrganizationDataReadAccessLevel.All);
            _contractRepository.Setup(x => x.GetContractsInOrganization(organizationId)).Returns(new List<ItContract> { contract1, contract2 }.AsQueryable());

            //Act
            var result = _sut.CanCreateNewContractWithName(name, organizationId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(overlapFound == false, result.Value);
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void CanCreateNewContractWithName_Returns_Forbidden_If_User_Does_Not_Have_Full_Access_In_Organization(OrganizationDataReadAccessLevel orgAccessThatFails)
        {
            //Arrange
            var name = A<string>();
            var organizationId = A<int>();

            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(orgAccessThatFails);

            //Act
            var result = _sut.CanCreateNewContractWithName(name, organizationId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void ValidateNewName_Returns_Success_If_No_Overlaps()
        {
            //Arrange
            var name = A<string>();
            var id = A<int>();
            var otherContract = new ItContract
            {
                Id = A<int>(),
                Name = A<string>()
            };

            var contract = new ItContract
            {
                Id = id,
                OrganizationId = A<int>(),
            };
            ExpectGetContractReturns(id, contract);
            _authorizationContext.Setup(x => x.AllowReads(contract)).Returns(true);
            _contractRepository.Setup(x => x.GetContractsInOrganization(contract.OrganizationId)).Returns(new List<ItContract> { otherContract, contract }.AsQueryable());

            //Act
            var result = _sut.ValidateNewName(id, name);

            //Assert
            Assert.True(result.IsNone);
        }

        [Fact]
        public void ValidateNewName_Returns_Error_If_Overlaps()
        {
            //Arrange
            var name = A<string>();
            var id = A<int>();
            var otherContract = new ItContract
            {
                Id = A<int>(),
                Name = name //overlapping name
            };

            var contract = new ItContract
            {
                Id = id,
                OrganizationId = A<int>(),
            };
            ExpectGetContractReturns(id, contract);
            _authorizationContext.Setup(x => x.AllowReads(contract)).Returns(true);
            _contractRepository.Setup(x => x.GetContractsInOrganization(contract.OrganizationId)).Returns(new List<ItContract> { otherContract, contract }.AsQueryable());

            //Act
            var result = _sut.ValidateNewName(id, name);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Conflict, result.Value.FailureType);
        }

        [Fact]
        public void ValidateNewName_Returns_Error_If_NotAllowedToAccessContract()
        {
            //Arrange
            var name = A<string>();
            var id = A<int>();

            var contract = new ItContract
            {
                Id = id,
                OrganizationId = A<int>(),
            };
            ExpectGetContractReturns(id, contract);
            _authorizationContext.Setup(x => x.AllowReads(contract)).Returns(false);

            //Act
            var result = _sut.ValidateNewName(id, name);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Fact]
        public void ValidateNewName_Returns_Error_If_ContractNotFound()
        {  //Arrange
            var name = A<string>();
            var id = A<int>();

            ExpectGetContractReturns(id, null);

            //Act
            var result = _sut.ValidateNewName(id, name);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Fact]
        public void Can_Get_Applied_Procurement_Plans()
        {
            //Arrange
            var itContracts = new (int? year, int? quarter)[]
            {
                (1,2),
                (null,1),
                (1,null),
                (null,null),
                (0,1),
                (0,1), //duplicate
                (1,1),
                (2,0),
                (2,0), //duplicate
            }.Select(x => new ItContract() { ProcurementPlanYear = x.year, ProcurementPlanQuarter = x.quarter }).ToList();

            //no duplicates and only valid unique combinations (no nulls), order by year and then by quarter
            var expectedResult =
                new (int year, int quarter)[]
                {
                    (0, 1),
                    (1, 1),
                    (1, 2),
                    (2, 0),
                };

            var orgId = A<int>();
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(OrganizationDataReadAccessLevel.All);
            _contractRepository.Setup(x => x.GetContractsInOrganization(orgId)).Returns(itContracts.AsQueryable());

            //Act
            var result = _sut.GetAppliedProcurementPlans(orgId);

            //Assert
            Assert.True(result.Ok);
            var activePlans = result.Value;
            Assert.Equal(expectedResult, activePlans);
        }

        private void ExpectCrossOrganizationReadAccess(CrossOrganizationDataReadAccessLevel crossOrganizationReadAccessLevel)
        {
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(crossOrganizationReadAccessLevel);
        }

        private List<ItContract> CreateListOfContracts(int numberOfItems)
        {
            var contracts = new List<ItContract>();
            for (int i = 0; i < numberOfItems; i++)
            {
                contracts.Add(new ItContract() { Id = A<int>() });
            }
            return contracts;
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
