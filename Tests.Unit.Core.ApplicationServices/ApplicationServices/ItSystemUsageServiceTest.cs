using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Serilog;
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
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IGenericRepository<SystemRelation>> _relationRepositoryMock;
        private readonly Mock<IGenericRepository<ItInterface>> _interfaceRepository;
        private readonly Mock<IDomainEvents> _domainEvents;

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
            _transactionManager = new Mock<ITransactionManager>();
            _relationRepositoryMock = new Mock<IGenericRepository<SystemRelation>>();
            _interfaceRepository = new Mock<IGenericRepository<ItInterface>>();
            _domainEvents = new Mock<IDomainEvents>();
            _sut = new ItSystemUsageService(
                _usageRepository.Object,
                _authorizationContext.Object,
                _systemRepository.Object,
                _contractRepository.Object,
                _optionsService.Object,
                _userContext.Object,
                _relationRepositoryMock.Object,
                _interfaceRepository.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                Mock.Of<ILogger>());
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
            _domainEvents.Verify(x => x.Raise(It.Is<SystemUsageDeleted>(ev => ev.DeletedSystemUsage == itSystemUsage)));
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
            AssertAddRelationError(result, OperationFailure.NotFound, "'From' not found");
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
            AssertAddRelationError(result, OperationFailure.BadInput, "'To' could not be found");
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
            ExpectGetAvailableOptionsReturns(source, frequencyId, Maybe<RelationFrequencyType>.None);

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
            ExpectGetAvailableOptionsReturns(source, frequencyId, new RelationFrequencyType { Id = frequencyId });
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
            ExpectGetAvailableOptionsReturns(source, frequencyId, new RelationFrequencyType { Id = frequencyId });
            _contractRepository.Setup(x => x.GetById(contractId)).Returns(new ItContract() { OrganizationId = source.OrganizationId });

            //Act
            var result = _sut.AddRelation(sourceId, destinationId, null, A<string>(), A<string>(), frequencyId, contractId);

            //Assert
            Assert.True(result.Ok);
            _usageRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void GetRelationsFrom_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var relations = _sut.GetRelationsFrom(id);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.NotFound, relations.Error.FailureType);
        }

        [Fact]
        public void GetRelationsFrom_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, false);

            //Act
            var relations = _sut.GetRelationsFrom(id);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.Forbidden, relations.Error.FailureType);
        }

        [Fact]
        public void GetRelationsFrom_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var systemRelations = new List<SystemRelation>() { CreateRelation(), CreateRelation() };
            var itSystemUsage = new ItSystemUsage { UsageRelations = systemRelations };
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, true);

            //Act
            var relations = _sut.GetRelationsFrom(id);

            //Assert
            Assert.True(relations.Ok);
            Assert.True(relations.Value.SequenceEqual(systemRelations));
        }

        [Fact]
        public void GetRelationsTo_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var relations = _sut.GetRelationsTo(id);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.NotFound, relations.Error.FailureType);
        }

        [Fact]
        public void GetRelationsTo_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, false);

            //Act
            var relations = _sut.GetRelationsTo(id);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.Forbidden, relations.Error.FailureType);
        }

        [Fact]
        public void GetRelationsTo_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var systemRelations = new List<SystemRelation>() { CreateRelation(), CreateRelation() };
            var itSystemUsage = new ItSystemUsage { UsedByRelations = systemRelations };
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, true);

            //Act
            var relations = _sut.GetRelationsTo(id);

            //Assert
            Assert.True(relations.Ok);
            Assert.True(relations.Value.SequenceEqual(systemRelations));
        }

        [Fact]
        public void GetRelation_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            var relationId = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var relation = _sut.GetRelationFrom(id, relationId);

            //Assert
            Assert.False(relation.Ok);
            Assert.Equal(OperationFailure.NotFound, relation.Error);
        }

        [Fact]
        public void GetRelation_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var relationId = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, false);

            //Act
            var relation = _sut.GetRelationFrom(id, relationId);

            //Assert
            Assert.False(relation.Ok);
            Assert.Equal(OperationFailure.Forbidden, relation.Error);
        }

        [Fact]
        public void GetRelation_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var relationId = A<int>();
            var systemRelation = CreateRelation();
            systemRelation.Id = relationId;
            var itSystemUsage = new ItSystemUsage { UsageRelations = new List<SystemRelation>() { CreateRelation(), systemRelation } };
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, true);

            //Act
            var relation = _sut.GetRelationFrom(id, relationId);

            //Assert
            Assert.True(relation.Ok);
            Assert.Same(systemRelation, relation.Value);
        }

        [Fact]
        public void RemoveRelation_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            var relationId = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var relation = _sut.RemoveRelation(id, relationId);

            //Assert
            Assert.False(relation.Ok);
            Assert.Equal(OperationFailure.NotFound, relation.Error);
        }

        [Fact]
        public void RemoveRelation_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var relationId = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, false);

            //Act
            var relation = _sut.RemoveRelation(id, relationId);

            //Assert
            Assert.False(relation.Ok);
            Assert.Equal(OperationFailure.Forbidden, relation.Error);
        }

        [Fact]
        public void RemoveRelation_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var relationId = A<int>();
            var systemRelation = CreateRelation();
            systemRelation.Id = relationId;
            var itSystemUsage = new ItSystemUsage { UsageRelations = new List<SystemRelation> { CreateRelation(), systemRelation } };
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            //Act
            var relation = _sut.RemoveRelation(id, relationId);

            //Assert
            Assert.True(relation.Ok);
            Assert.Same(systemRelation, relation.Value);
            _relationRepositoryMock.Verify(x => x.DeleteWithReferencePreload(systemRelation), Times.Once);
            _relationRepositoryMock.Verify(x => x.Save(), Times.Once);
            _usageRepository.Verify(x => x.Save(), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(26)]
        public void GetSystemUsagesWhichCanBeRelatedTo_Returns_BadInput_For_PageSize(int pageSize)
        {
            //Arrange
            var id = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var result = _sut.GetSystemUsagesWhichCanBeRelatedTo(id, Maybe<string>.None, pageSize);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetSystemUsagesWhichCanBeRelatedTo_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var result = _sut.GetSystemUsagesWhichCanBeRelatedTo(id, Maybe<string>.None, 2);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetSystemUsagesWhichCanBeRelatedTo_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, false);

            //Act
            var result = _sut.GetSystemUsagesWhichCanBeRelatedTo(id, Maybe<string>.None, 2);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetSystemUsagesWhichCanBeRelatedTo_Without_NameContent_Returns_AvailableSystemUsages()
        {
            //Arrange
            var organizationId = A<int>();
            var itSystem1 = CreateItSystem();
            var itSystem2 = CreateItSystem();
            var itSystem3 = CreateItSystem();
            var fromItSystemUsage = CreateSystemUsage(organizationId, itSystem1);
            var includedSystemUsage1 = CreateSystemUsage(organizationId, itSystem2);
            var includedSystemUsage2 = CreateSystemUsage(organizationId, itSystem3);

            ExpectGetUsageByKeyReturns(fromItSystemUsage.Id, fromItSystemUsage);
            ExpectAllowReadReturns(fromItSystemUsage, true);
            _systemRepository.Setup(x => x.GetSystemsInUse(organizationId)).Returns(new[] { itSystem1, itSystem2, itSystem3 }.AsQueryable());
            _usageRepository.Setup(x => x.AsQueryable()).Returns(new[] { includedSystemUsage1, includedSystemUsage2, fromItSystemUsage }.AsQueryable());

            //Act
            var result = _sut.GetSystemUsagesWhichCanBeRelatedTo(fromItSystemUsage.Id, Maybe<string>.None, 3);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(new[] { includedSystemUsage1.Id, includedSystemUsage2.Id }.OrderBy(x => x), result.Value.Select(x => x.Id).OrderBy(x => x));
        }

        private ItSystemUsage CreateSystemUsage(int organizationId, ItSystem itSystem)
        {
            return new ItSystemUsage
            {
                OrganizationId = organizationId,
                Id = A<int>(),
                ItSystemId = itSystem.Id,
                ItSystem = itSystem
            };
        }

        private ItSystem CreateItSystem()
        {
            return new ItSystem
            {
                Name = A<string>(),
                Id = A<int>()
            };
        }

        private SystemRelation CreateRelation()
        {
            var systemRelation = new SystemRelation(new ItSystemUsage());
            systemRelation.SetRelationTo(new ItSystemUsage { Id = A<int>() });
            return systemRelation;
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

        private void ExpectGetAvailableOptionsReturns(ItSystemUsage source, int optionId, Maybe<RelationFrequencyType> response)
        {
            _optionsService.Setup(x => x.GetAvailableOption(source.OrganizationId, optionId)).Returns(response);
        }

        private void ExpectAllowModifyReturns(ItSystemUsage source, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(source)).Returns(value);
        }

        private void ExpectAllowReadReturns(ItSystemUsage itSystemUsage, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(itSystemUsage)).Returns(value);
        }
    }
}
