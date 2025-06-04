using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;

using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageServiceRelationTest : WithAutoFixture
    {
        private const int FromSystemUsageId = 1;
        private const int ToSystemUsageId = 2;
        private const int FromSystemRelationId = 11;
        private const int Interface1Id = 100;
        private const int Interface2Id = 101;
        private const int Contract1Id = 200;
        private const int Contract2Id = 201;
        private const int FrequencyType1Id = 300;
        private const int FrequencyType2Id = 301;

        private readonly Mock<IGenericRepository<ItSystemUsage>> _mockSystemUsageRepository;
        private readonly Mock<IAuthorizationContext> _mockAuthorizationContext;
        private readonly Mock<IItSystemRepository> _mockSystemRepository;
        private readonly Mock<IItContractRepository> _mockContractRepository;
        private readonly Mock<IOptionsService<SystemRelation, RelationFrequencyType>> _mockOptionsService;
        private readonly Mock<IGenericRepository<SystemRelation>> _mockSystemRelationRepository;
        private readonly Mock<ITransactionManager> _mockTransactionManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly ItsystemUsageRelationsService _sut;
        private readonly Mock<IGenericRepository<ItInterface>> _interfaceRepository;

        public ItSystemUsageServiceRelationTest()
        {
            _mockSystemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _mockAuthorizationContext = new Mock<IAuthorizationContext>();
            _mockSystemRepository = new Mock<IItSystemRepository>();
            _mockContractRepository = new Mock<IItContractRepository>();
            _mockOptionsService = new Mock<IOptionsService<SystemRelation, RelationFrequencyType>>();
            _mockSystemRelationRepository = new Mock<IGenericRepository<SystemRelation>>();
            _mockTransactionManager = new Mock<ITransactionManager>();
            _mockLogger = new Mock<ILogger>();
            _interfaceRepository = new Mock<IGenericRepository<ItInterface>>();
            _mockOptionsService.Setup(x => x.GetAvailableOption(It.IsAny<int>(), It.IsAny<int>())).Returns(Maybe<RelationFrequencyType>.None);
            _sut = new ItsystemUsageRelationsService(
                _mockSystemUsageRepository.Object,
                _mockAuthorizationContext.Object,
                _mockSystemRepository.Object,
                _mockContractRepository.Object,
                _mockOptionsService.Object,
                _mockSystemRelationRepository.Object,
                _interfaceRepository.Object,
                _mockTransactionManager.Object,
                Mock.Of<IDomainEvents>(),
                _mockLogger.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ModifyRelation_GivenAnyUserWithItSystemWriteAccess_AllowsModifications(bool allowModifications)
        {
            // Arrange
            var fromSystemUsage = new ItSystemUsage { Id = FromSystemUsageId };
            var toSystemUsage = new ItSystemUsage { Id = ToSystemUsageId };
            var systemRelation = new SystemRelation(fromSystemUsage) { Id = FromSystemRelationId, ToSystemUsage = toSystemUsage };
            fromSystemUsage.UsageRelations = new List<SystemRelation> { systemRelation };
            _mockSystemRelationRepository.Setup(r => r.GetByKey(FromSystemRelationId)).Returns(systemRelation);
            _mockSystemUsageRepository.Setup(r => r.GetByKey(FromSystemUsageId)).Returns(fromSystemUsage);
            _mockSystemUsageRepository.Setup(r => r.GetByKey(ToSystemUsageId)).Returns(toSystemUsage);
            _mockAuthorizationContext.Setup(c => c.AllowModify(fromSystemUsage)).Returns(allowModifications);

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, ToSystemUsageId, null, null, null, null, null);

            // Assert
            Assert.True(allowModifications ? result.Ok : result.Error.FailureType == OperationFailure.Forbidden);
        }

        [Fact]
        public void ModifyRelation_GivenSourceSystemUsingTarget_WhenTargetIsReplaced_RelationIsUpdated()
        {
            // Arrange
            const int replacementSystemUsageId = 3;
            var mockReplacementSystemUsage = SetupMockSystemUsage(replacementSystemUsageId);
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            systemUsages.Add(replacementSystemUsageId, mockReplacementSystemUsage.Object);
            SetupSystemUsageRepository(systemUsages);

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, replacementSystemUsageId, null, null, null, null, null);

            // Assert
            Assert.True(result.Ok);
            var modifiedSystemRelation = mockFromSystemUsage.Object.UsageRelations.First(r => r.Id == FromSystemRelationId);
            Assert.Equal(mockReplacementSystemUsage.Object, modifiedSystemRelation.ToSystemUsage);
            Assert.Null(modifiedSystemRelation.RelationInterface);
        }

        [Fact]
        public void ModifyRelation_GivenSystemUsageAndInterfaceChange_ValidatesInterfaceIsExposedByTargetSystemAndUpdates()
        {
            // Arrange
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            SetupSystemUsageRepository(systemUsages);
            _interfaceRepository.Setup(x => x.GetByKey(Interface2Id)).Returns(new ItInterface() { Id = Interface2Id });

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, ToSystemUsageId, "", "", Interface2Id, null, null);

            // Assert
            Assert.True(result.Ok);
            Assert.Equal(Interface2Id, GetModifiedSystemRelation(mockFromSystemUsage).RelationInterface.Id);
        }

        [Fact]
        public void ModifyRelation_GivenSystemUsageAndContractChange_ValidatesContractAndUpdates()
        {
            // Arrange
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            SetupSystemUsageRepository(systemUsages);
            SetupContractRepository();

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, ToSystemUsageId, "", "", null, Contract2Id, null);

            // Assert
            Assert.True(result.Ok);
            Assert.Equal(Contract2Id, GetModifiedSystemRelation(mockFromSystemUsage).AssociatedContract.Id);
        }

        [Fact]
        public void ModifyRelation_GivenSystemUsageAndFrequencyChange_ValidatesFrequencyAndUpdates()
        {
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            SetupSystemUsageRepository(systemUsages);
            _mockOptionsService.Setup(x => x.GetAvailableOption(It.IsAny<int>(), FrequencyType2Id)).Returns(new RelationFrequencyType() { Id = FrequencyType2Id });

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, ToSystemUsageId, "", "",
                null, null, FrequencyType2Id);

            // Assert
            Assert.True(result.Ok);
            Assert.Equal(FrequencyType2Id, GetModifiedSystemRelation(mockFromSystemUsage).UsageFrequency.Id);
        }

        [Fact]
        public void ModifyRelation_GivenSystemUsageAndChangedDescriptionAndReference_TheseFieldsAreValidatedAndUpdated()
        {
            // Arrange
            const string changedDescription = "ChangedDescription";
            const string changedReference = "ChangedReference";
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            SetupSystemUsageRepository(systemUsages);

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, ToSystemUsageId, changedDescription, changedReference, null, null, null);

            // Assert
            Assert.True(result.Ok);
            Assert.Equal(changedDescription, GetModifiedSystemRelation(mockFromSystemUsage).Description);
            Assert.Equal(changedReference, GetModifiedSystemRelation(mockFromSystemUsage).Reference);
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
            _mockContractRepository.Setup(x => x.GetById(contractId)).Returns(default(ItContract));

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
            _mockContractRepository.Setup(x => x.GetById(contractId)).Returns(new ItContract() { OrganizationId = source.OrganizationId });

            //Act
            var result = _sut.AddRelation(sourceId, destinationId, null, A<string>(), A<string>(), frequencyId, contractId);

            //Assert
            Assert.True(result.Ok);
            _mockSystemUsageRepository.Verify(x => x.Save(), Times.Once);
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
        public void GetRelationsAssociatedWithContract_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            _mockContractRepository.Setup(x => x.GetById(id)).Returns(default(ItContract));

            //Act
            var relations = _sut.GetRelationsAssociatedWithContract(id);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.NotFound, relations.Error.FailureType);
        }

        [Fact]
        public void GetRelationsAssociatedWithContract_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itContract = new ItContract();
            _mockContractRepository.Setup(x => x.GetById(id)).Returns(itContract);
            ExpectAllowReadReturns(itContract, false);

            //Act
            var relations = _sut.GetRelationsAssociatedWithContract(id);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.Forbidden, relations.Error.FailureType);
        }

        [Fact]
        public void GetRelationsAssociatedWithContract_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var associatedSystemRelations = new List<SystemRelation>() { CreateRelation(), CreateRelation() };
            var itContract = new ItContract { AssociatedSystemRelations = associatedSystemRelations };
            _mockContractRepository.Setup(x => x.GetById(id)).Returns(itContract);
            ExpectAllowReadReturns(itContract, true);

            //Act
            var relations = _sut.GetRelationsAssociatedWithContract(id);

            //Assert
            Assert.True(relations.Ok);
            Assert.True(relations.Value.SequenceEqual(associatedSystemRelations));
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
            Assert.Equal(OperationFailure.NotFound, relation.Error.FailureType);
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
            _mockTransactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, true);

            //Act
            var relation = _sut.RemoveRelation(id, relationId);

            //Assert
            Assert.True(relation.Ok);
            Assert.Same(systemRelation, relation.Value);
            _mockSystemRelationRepository.Verify(x => x.DeleteWithReferencePreload(systemRelation), Times.Once);
            _mockSystemRelationRepository.Verify(x => x.Save(), Times.Once);
            _mockSystemUsageRepository.Verify(x => x.Save(), Times.Once);
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
            _mockSystemRepository.Setup(x => x.GetSystemsInUse(organizationId)).Returns(new[] { itSystem1, itSystem2, itSystem3 }.AsQueryable());
            _mockSystemUsageRepository.Setup(x => x.AsQueryable()).Returns(new[] { includedSystemUsage1, includedSystemUsage2, fromItSystemUsage }.AsQueryable());

            //Act
            var result = _sut.GetSystemUsagesWhichCanBeRelatedTo(fromItSystemUsage.Id, Maybe<string>.None, 3);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(new[] { includedSystemUsage1.Id, includedSystemUsage2.Id }.OrderBy(x => x), result.Value.Select(x => x.Id).OrderBy(x => x));
        }

        [Theory]
        [InlineData(-1, 1)]
        [InlineData(0, 0)]
        [InlineData(0, 101)]
        public void GetRelationsDefinedInOrganization_Returns_BadInput(int pageNumber, int pageSize)
        {
            //Arrange
            var organizationId = A<int>();

            //Act
            var relations = _sut.GetRelationsDefinedInOrganization(organizationId, pageNumber, pageSize);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.BadInput, relations.Error.FailureType);
        }

        [Theory, Description("Full organization access is required to access local data")]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        public void GetRelationsDefinedInOrganization_Returns_Forbidden(OrganizationDataReadAccessLevel accessLevelInOrganization)
        {
            //Arrange
            var organizationId = A<int>();
            const int pageNumber = 0;
            const int pageSize = 100;
            ExpectGetOrganizationReadAccessReturns(organizationId, accessLevelInOrganization);

            //Act
            var relations = _sut.GetRelationsDefinedInOrganization(organizationId, pageNumber, pageSize);

            //Assert
            Assert.False(relations.Ok);
            Assert.Equal(OperationFailure.Forbidden, relations.Error.FailureType);
        }

        [Fact]
        public void GetRelationsDefinedInOrganization_Returns_Ok()
        {
            //Arrange
            var organizationId = A<int>();
            var differentOrganizationId = organizationId + 1;

            var relationsFromFirst = new List<SystemRelation>() { CreateRelation(), CreateRelation() };
            var relationsFromSecond = new List<SystemRelation>() { CreateRelation(), CreateRelation() };
            const int pageNumber = 0;
            const int pageSize = 100;

            ExpectGetOrganizationReadAccessReturns(organizationId, OrganizationDataReadAccessLevel.All);
            _mockSystemUsageRepository.Setup(x => x.AsQueryable()).Returns(new[]
            {
                CreateSystemUsageWithRelations(relationsFromFirst, organizationId),
                CreateSystemUsageWithRelations(relationsFromSecond, organizationId),

                //This one should be excluded from the results - invalid organization
                CreateSystemUsageWithRelations(new List<SystemRelation> {CreateRelation()}, differentOrganizationId)
            }.AsQueryable());

            //Act
            var relations = _sut.GetRelationsDefinedInOrganization(organizationId, pageNumber, pageSize);

            //Assert
            Assert.True(relations.Ok);
            Assert.True(relations.Value.OrderBy(x => x.Id).SequenceEqual(relationsFromFirst.Concat(relationsFromSecond).OrderBy(x => x.Id)));
        }

        [Fact]
        public void GetRelationsDefinedInOrganization_Returns_Ok_With_Paging()
        {
            //Arrange
            var organizationId = A<int>();
            var relationsFromFirst = new List<SystemRelation>() { CreateRelation(), CreateRelation() };
            const int pageTwoPageNumber = 1;
            const int pageSize = 1;

            ExpectGetOrganizationReadAccessReturns(organizationId, OrganizationDataReadAccessLevel.All);
            _mockSystemUsageRepository.Setup(x => x.AsQueryable()).Returns(new[]
            {
                CreateSystemUsageWithRelations(relationsFromFirst, organizationId),
            }.AsQueryable());

            //Act
            var relations = _sut.GetRelationsDefinedInOrganization(organizationId, pageTwoPageNumber, pageSize); //skips first page 

            //Assert
            Assert.True(relations.Ok);
            Assert.Same(relationsFromFirst.OrderBy(x => x.Id).Last(), relations.Value.Single());
        }

        #region Helpers
        private void ExpectGetUsageByKeyReturns(int id, ItSystemUsage itSystemUsage)
        {
            _mockSystemUsageRepository.Setup(x => x.GetByKey(id)).Returns(itSystemUsage);
        }
        private void ExpectAllowModifyReturns(ItSystemUsage source, bool value)
        {
            _mockAuthorizationContext.Setup(x => x.AllowModify(source)).Returns(value);
        }

        private void ExpectAllowReadReturns(IEntity entity, bool value)
        {
            _mockAuthorizationContext.Setup(x => x.AllowReads(entity)).Returns(value);
        }

        private static ItSystemUsage CreateSystemUsageWithRelations(List<SystemRelation> relationsFromFirst, int organizationId)
        {
            return new ItSystemUsage
            {
                UsageRelations = relationsFromFirst,
                OrganizationId = organizationId
            };
        }

        private void ExpectGetOrganizationReadAccessReturns(int organizationId, OrganizationDataReadAccessLevel accessLevelInOrganization)
        {
            _mockAuthorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(accessLevelInOrganization);
        }
        private void ExpectGetAvailableOptionsReturns(ItSystemUsage source, int optionId, Maybe<RelationFrequencyType> response)
        {
            _mockOptionsService.Setup(x => x.GetAvailableOption(source.OrganizationId, optionId)).Returns(response);
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

        private Dictionary<int, ItSystemUsage> SetupSystemRelationWithIngredients(out Mock<ItSystemUsage> mockFromSystemUsage)
        {
            mockFromSystemUsage = SetupMockSystemUsage(FromSystemUsageId);
            var mockToSystemUsage = SetupMockSystemUsage(ToSystemUsageId);
            var fromSystemUsage = mockFromSystemUsage.Object;
            var systemUsages = new Dictionary<int, ItSystemUsage>
            {
                {
                    FromSystemUsageId, fromSystemUsage
                },
                {
                    ToSystemUsageId, mockToSystemUsage.Object
                }
            };
            SetupSystemRelation(FromSystemRelationId, mockFromSystemUsage, mockToSystemUsage);
            _mockAuthorizationContext.Setup(c => c.AllowModify(fromSystemUsage)).Returns(true);
            return systemUsages;
        }

        private void SetupSystemRelation(int systemRelationId, Mock<ItSystemUsage> sourceSystemUsage, Mock<ItSystemUsage> targetSystemUsage)
        {
            var usageSystemRelation = new SystemRelation(sourceSystemUsage.Object)
            {
                Id = systemRelationId,
                Description = "MyDescription",
                Reference = "https://dummy.dk",
                RelationInterface = new ItInterface { Id = Interface1Id },
                AssociatedContract = new ItContract { Id = Contract1Id },
                UsageFrequency = new RelationFrequencyType { Id = FrequencyType1Id }
            };
            usageSystemRelation.SetRelationTo(targetSystemUsage.Object);
            sourceSystemUsage.SetupGet(u => u.UsageRelations).Returns(new List<SystemRelation> { usageSystemRelation });
            var itInterfaceExhibits = new List<ItInterfaceExhibit>
            {
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = Interface1Id }},
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = Interface2Id }}
            };
            var mockTargetItSystem = new Mock<ItSystem>();
            mockTargetItSystem.SetupGet(s => s.ItInterfaceExhibits).Returns(itInterfaceExhibits);
            targetSystemUsage.SetupGet(u => u.ItSystem).Returns(mockTargetItSystem.Object);
            _mockSystemRelationRepository.Setup(r => r.GetByKey(usageSystemRelation.Id)).Returns(usageSystemRelation);
        }

        private void SetupSystemUsageRepository(IReadOnlyDictionary<int, ItSystemUsage> systemUsages)
        {
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<object[]>()))
                .Returns((object[] key) => systemUsages[(int)key[0]]);
        }

        private void SetupContractRepository()
        {
            var contracts = new Dictionary<int, ItContract>
            {
                {Contract1Id, new ItContract {Id = Contract1Id}},
                {Contract2Id, new ItContract {Id = Contract2Id}}
            };
            _mockContractRepository.Setup(r => r.GetById(It.IsAny<int>()))
                .Returns((int key) => contracts[key]);
        }

        private static SystemRelation GetModifiedSystemRelation(IMock<ItSystemUsage> mockFromSystemUsage)
        {
            var modifiedSystemRelation = mockFromSystemUsage.Object.UsageRelations.First(r => r.Id == FromSystemRelationId);
            return modifiedSystemRelation;
        }

        private static Mock<ItSystemUsage> SetupMockSystemUsage(int id)
        {
            var mockSystemUsage1 = new Mock<ItSystemUsage>();
            mockSystemUsage1.Object.Id = id;
            return mockSystemUsage1;
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

        #endregion
    }
}
