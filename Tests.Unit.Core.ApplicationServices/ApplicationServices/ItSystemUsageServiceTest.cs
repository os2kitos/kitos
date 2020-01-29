using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class ItSystemUsageServiceTest : WithAutoFixture
    {
        private readonly ItSystemUsageService _sut;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _usageRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<IItContractRepository> _contractRepository;
        private readonly Mock<IOptionsService<SystemRelation, RelationFrequencyType>> _optionsService;
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly User _activeUser;

        public ItSystemUsageServiceTest()
        {
            _usageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _contractRepository = new Mock<IItContractRepository>();
            _optionsService = new Mock<IOptionsService<SystemRelation, RelationFrequencyType>>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _activeUser = new User();
            _userContext.Setup(x => x.UserEntity).Returns(_activeUser);
            _sut = new ItSystemUsageService(
                _usageRepository.Object,
                _authorizationContext.Object,
                _systemRepository.Object,
                _contractRepository.Object,
                _optionsService.Object,
                _userContext.Object);
        }

        [Fact]
        public void Add_Returns_Conflict_If_System_Already_In_Use()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var systemUsage = SetupRepositoryQueryWith(organizationId, systemId);

            //Act
            var result = _sut.Add(systemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Conflict, result.Error);
        }

        [Fact]
        public void Add_Returns_Forbidden_Not_Allowed_To_Create()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(false);

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Add_Returns_BadInput_If_ItSystemNotFound()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>() };
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(default(ItSystem));

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void Add_Returns_Forbidden_If_ReadAccess_To_ItSystem_Is_Declined()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>(), OrganizationId = A<int>() };
            var itSystem = new ItSystem();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(itSystem);
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(false);

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Add_Returns_Forbidden_If_Usage_Of_Local_System_In_Other_Org_Is_Attempted()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>(), OrganizationId = A<int>() };
            var itSystem = new ItSystem() { OrganizationId = itSystemUsage.OrganizationId + 1, AccessModifier = AccessModifier.Local };
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(itSystem);
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(true);

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Add_Returns_Ok(bool sameOrg)
        {
            //Arrange
            var objectOwner = new User();
            var input = new ItSystemUsage
            {
                ItSystemId = A<int>(),
                OrganizationId = A<int>(),
                DataLevel = A<DataSensitivityLevel>(),
                ContainsLegalInfo = A<DataOptions>(),
                AssociatedDataWorkers = new List<ItSystemUsageDataWorkerRelation> { new ItSystemUsageDataWorkerRelation(), new ItSystemUsageDataWorkerRelation() }

            };
            var associatedItSystem = new ItSystem
            {
                OrganizationId = sameOrg ? input.OrganizationId : input.OrganizationId + 1,
                AccessModifier = AccessModifier.Public
            };
            var usageCreatedByRepo = new ItSystemUsage();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(input)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(input.ItSystemId)).Returns(associatedItSystem);
            _authorizationContext.Setup(x => x.AllowReads(associatedItSystem)).Returns(true);
            _usageRepository.Setup(x => x.Create()).Returns(usageCreatedByRepo);

            //Act
            var result = _sut.Add(input, objectOwner);

            //Assert
            Assert.True(result.Ok);
            var createdUsage = result.Value;
            Assert.NotSame(input, createdUsage);
            Assert.Same(usageCreatedByRepo, createdUsage);
            Assert.Same(objectOwner, createdUsage.ObjectOwner);
            Assert.Equal(input.OrganizationId, createdUsage.OrganizationId);
            Assert.Equal(input.DataLevel, createdUsage.DataLevel);
            Assert.Equal(input.ItSystemId, createdUsage.ItSystemId);
            Assert.Equal(input.ContainsLegalInfo, createdUsage.ContainsLegalInfo);
            Assert.Equal(input.AssociatedDataWorkers, createdUsage.AssociatedDataWorkers);
            _usageRepository.Verify(x => x.Insert(usageCreatedByRepo), Times.Once);
            _usageRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            _authorizationContext.Setup(x => x.AllowDelete(itSystemUsage)).Returns(false);

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Delete_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            _authorizationContext.Setup(x => x.AllowDelete(itSystemUsage)).Returns(true);

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystemUsage, result.Value);
            _usageRepository.Verify(x => x.DeleteByKeyWithReferencePreload(id), Times.Once);
            _usageRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void GetByOrganizationAndSystemId_Returns_Nothing()
        {
            //Arrange
            var systemId = A<int>();
            var organizationId = A<int>();
            SetupRepositoryQueryWith(organizationId + 1, systemId + 1);

            //Act
            var result = _sut.GetByOrganizationAndSystemId(organizationId, systemId);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetByOrganizationAndSystemId_Returns()
        {
            //Arrange
            var systemId = A<int>();
            var organizationId = A<int>();
            var systemUsage = SetupRepositoryQueryWith(organizationId, systemId);

            //Act
            var result = _sut.GetByOrganizationAndSystemId(organizationId, systemId);

            //Assert
            Assert.Same(systemUsage, result);
        }

        [Fact]
        public void GetById_Returns()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);

            //Act
            var result = _sut.GetById(id);

            //Assert
            Assert.Same(itSystemUsage, result);
        }

        [Fact]
        public void AddRelation_Returns_Error_If_Source_Is_Not_Found()
        {
            //Arrange
            var sourceId = A<int>();
            ExpectGetUsageByKeyReturns(sourceId, null);

            //Act
            var result = _sut.AddRelation(sourceId, A<int>(), null, A<string>(), A<string>(), null, null);

            //Assert
            AssertAddRelationError(result, OperationFailure.NotFound, "Source not found");
        }

        [Fact]
        public void AddRelation_Returns_Error_If_Destination_Is_Not_Found()
        {
            //Arrange
            var sourceId = A<int>();
            var destinationId = A<int>();
            ExpectGetUsageByKeyReturns(sourceId, new ItSystemUsage());
            ExpectGetUsageByKeyReturns(destinationId, null);


            //Act
            var result = _sut.AddRelation(sourceId, destinationId, null, A<string>(), A<string>(), null, null);

            //Assert
            AssertAddRelationError(result, OperationFailure.BadInput, "Destination could not be found");
        }

        [Fact]
        public void AddRelation_Returns_Error_If_Modification_Is_Denied_On_Source()
        {
            //Arrange
            var sourceId = A<int>();
            var destinationId = A<int>();
            var source = new ItSystemUsage
            {
                OrganizationId = A<int>()
            };
            ExpectGetUsageByKeyReturns(sourceId, source);
            ExpectGetUsageByKeyReturns(destinationId, new ItSystemUsage());
            ExpectAllowModifyReturns(source, false);

            //Act
            var result = _sut.AddRelation(sourceId, destinationId, null, A<string>(), A<string>(), A<int>(), null);

            //Assert
            AssertAddRelationError(result, OperationFailure.Forbidden, Maybe<string>.None);
        }

        [Fact]
        public void AddRelation_Returns_Error_If_Provided_Frequency_Is_Not_Available()
        {
            //Arrange
            var sourceId = A<int>();
            var destinationId = A<int>();
            var frequencyId = A<int>();
            var source = new ItSystemUsage
            {
                OrganizationId = A<int>()
            };
            ExpectGetUsageByKeyReturns(sourceId, source);
            ExpectGetUsageByKeyReturns(destinationId, new ItSystemUsage());
            ExpectAllowModifyReturns(source, true);
            ExpectGetAvailableOptionsReturns(source, new RelationFrequencyType { Id = frequencyId + 1 });

            //Act
            var result = _sut.AddRelation(sourceId, destinationId, null, A<string>(), A<string>(), frequencyId, null);

            //Assert
            AssertAddRelationError(result, OperationFailure.BadInput, "Frequency type is not available in the organization");
        }

        [Fact]
        public void AddRelation_Returns_Error_If_Provided_Contract_Is_Not_found()
        {
            //Arrange
            var sourceId = A<int>();
            var destinationId = A<int>();
            var frequencyId = A<int>();
            var contractId = A<int>();
            var source = new ItSystemUsage
            {
                Id = sourceId,
                OrganizationId = A<int>()
            };
            ExpectGetUsageByKeyReturns(sourceId, source);
            ExpectGetUsageByKeyReturns(destinationId, new ItSystemUsage() { Id = sourceId + 1, OrganizationId = source.OrganizationId });
            ExpectAllowModifyReturns(source, true);
            ExpectGetAvailableOptionsReturns(source, new RelationFrequencyType { Id = frequencyId });
            _contractRepository.Setup(x => x.GetById(contractId)).Returns(default(ItContract));

            //Act
            var result = _sut.AddRelation(sourceId, destinationId, null, A<string>(), A<string>(), frequencyId, contractId);

            //Assert
            AssertAddRelationError(result, OperationFailure.BadInput, "Contract id does not point to a valid contract");
        }

        [Fact]
        public void AddRelation_Returns_Ok()
        {
            //Arrange
            var sourceId = A<int>();
            var destinationId = A<int>();
            var frequencyId = A<int>();
            var contractId = A<int>();
            var source = new ItSystemUsage
            {
                Id = sourceId,
                OrganizationId = A<int>()
            };
            ExpectGetUsageByKeyReturns(sourceId, source);
            ExpectGetUsageByKeyReturns(destinationId, new ItSystemUsage() { Id = sourceId + 1, OrganizationId = source.OrganizationId });
            ExpectAllowModifyReturns(source, true);
            ExpectGetAvailableOptionsReturns(source, new RelationFrequencyType { Id = frequencyId });
            _contractRepository.Setup(x => x.GetById(contractId)).Returns(new ItContract() { OrganizationId = source.OrganizationId });

            //Act
            var result = _sut.AddRelation(sourceId, destinationId, null, A<string>(), A<string>(), frequencyId, contractId);

            //Assert
            Assert.True(result.Ok);
            _usageRepository.Verify(x => x.Save(), Times.Once);
        }

        private static void AssertAddRelationError(Result<SystemRelation, OperationError> result, OperationFailure operationFailure, Maybe<string> message)
        {
            Assert.False(result.Ok);
            Assert.Equal(operationFailure, result.Error.FailureType);
            Assert.Equal(message, result.Error.Message);
        }

        private ItSystemUsage SetupRepositoryQueryWith(int organizationId, int systemId)
        {
            var itSystemUsage = new ItSystemUsage { OrganizationId = organizationId, ItSystemId = systemId };

            _usageRepository.Setup(x => x.AsQueryable())
                .Returns(new[] { itSystemUsage }.AsQueryable());

            return itSystemUsage;
        }

        private void SetupRepositoryQueryWith(IEnumerable<ItSystemUsage> response = null)
        {
            response = response ?? new ItSystemUsage[0];
            _usageRepository.Setup(x => x.AsQueryable()).Returns(response.AsQueryable());
        }

        private void ExpectGetUsageByKeyReturns(int id, ItSystemUsage itSystemUsage)
        {
            _usageRepository.Setup(x => x.GetByKey(id)).Returns(itSystemUsage);
        }

        private void ExpectGetAvailableOptionsReturns(ItSystemUsage source, params RelationFrequencyType[] results)
        {
            _optionsService.Setup(x => x.GetAvailableOptions(source.OrganizationId)).Returns(new List<RelationFrequencyType>(results));
        }

        private void ExpectAllowModifyReturns(ItSystemUsage source, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(source)).Returns(value);
        }
    }
}
