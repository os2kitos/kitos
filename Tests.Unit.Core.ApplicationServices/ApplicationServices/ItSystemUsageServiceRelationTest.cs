using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
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

        private readonly Mock<IGenericRepository<ItSystemUsage>> _mockSystemUsageRepository;
        private readonly Mock<IAuthorizationContext> _mockAuthorizationContext;
        private readonly Mock<IItSystemRepository> _mockSystemRepository;
        private readonly Mock<IItContractRepository> _mockContractRepository;
        private readonly Mock<IOptionsService<SystemRelation, RelationFrequencyType>> _mockOptionsService;
        private readonly Mock<IOrganizationalUserContext> _mockOrganizationalUserContext;
        private readonly Mock<IGenericRepository<SystemRelation>> _mockSystemRelationRepository;
        private readonly Mock<ITransactionManager> _mockTransactionManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly ItSystemUsageService _sut;


        public ItSystemUsageServiceRelationTest()
        {
            _mockSystemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _mockAuthorizationContext = new Mock<IAuthorizationContext>();
            _mockSystemRepository = new Mock<IItSystemRepository>();
            _mockContractRepository = new Mock<IItContractRepository>();
            _mockOptionsService = new Mock<IOptionsService<SystemRelation, RelationFrequencyType>>();
            _mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
            _mockSystemRelationRepository = new Mock<IGenericRepository<SystemRelation>>();
            _mockTransactionManager = new Mock<ITransactionManager>();
            _mockLogger = new Mock<ILogger>();
            _mockOrganizationalUserContext.SetupGet(c => c.UserEntity).Returns(new User());
            _sut = new ItSystemUsageService(
                _mockSystemUsageRepository.Object,
                _mockAuthorizationContext.Object,
                _mockSystemRepository.Object,
                _mockContractRepository.Object,
                _mockOptionsService.Object,
                _mockOrganizationalUserContext.Object,
                _mockSystemRelationRepository.Object,
                _mockTransactionManager.Object,
                _mockLogger.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        private void ModifyRelation_GivenAnyUserWithItSystemWriteAccess_AllowsModifications(bool allowModifications)
        {
            // Arrange
            var fromSystemUsage = new ItSystemUsage { Id = FromSystemUsageId };
            var toSystemUsage = new ItSystemUsage { Id = ToSystemUsageId };
            var systemRelation = new SystemRelation(fromSystemUsage) { Id = FromSystemRelationId };
            fromSystemUsage.UsageRelations = new List<SystemRelation> { systemRelation };
            _mockSystemUsageRepository.Setup(r => r.GetByKey(FromSystemUsageId)).Returns(fromSystemUsage);
            _mockSystemUsageRepository.Setup(r => r.GetByKey(ToSystemUsageId)).Returns(toSystemUsage);
            _mockAuthorizationContext.Setup(c => c.AllowModify(fromSystemUsage)).Returns(allowModifications);

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId,ToSystemUsageId);

            // Assert
            Assert.True(allowModifications ? result.Ok : result.Error.FailureType == OperationFailure.Forbidden);
        }

        [Fact]
        private void ModifyRelation_GivenSourceSystemUsingTarget_WhenTargetIsReplaced_RelationIsUpdated()
        {
            // Arrange
            const int replacementSystemUsageId = 3;
            var mockReplacementSystemUsage = SetupMockSystemUsage(replacementSystemUsageId);
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            systemUsages.Add(replacementSystemUsageId, mockReplacementSystemUsage.Object);
            SetupSystemUsageRepository(systemUsages);

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, replacementSystemUsageId, null);

            // Assert
            Assert.True(result.Ok);
            var modifiedSystemRelation = mockFromSystemUsage.Object.UsageRelations.First(r => r.Id == FromSystemRelationId);
            Assert.Equal(mockReplacementSystemUsage.Object, modifiedSystemRelation.ToSystemUsage);
            Assert.Null(modifiedSystemRelation.RelationInterface);
        }

        [Fact]
        private void ModifyRelation_GivenSystemUsageAndInterfaceChange_ValidatesInterfaceIsExposedByTargetSystemAndUpdates()
        {
            // Arrange
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            SetupSystemUsageRepository(systemUsages);

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, ToSystemUsageId, Interface2Id);

            // Assert
            Assert.True(result.Ok);
            Assert.Equal(Interface2Id, GetModifiedSystemRelation(mockFromSystemUsage).RelationInterface.Id);
        }

        private static SystemRelation GetModifiedSystemRelation(Mock<ItSystemUsage> mockFromSystemUsage)
        {
            var modifiedSystemRelation = mockFromSystemUsage.Object.UsageRelations.First(r => r.Id == FromSystemRelationId);
            return modifiedSystemRelation;
        }

        [Fact]
        private void ModifyRelation_GivenSystemUsageAndContractChange_ValidatesContractFromSameMunicipalityAndUpdates()
        {
            // Arrange
            var systemUsages = SetupSystemRelationWithIngredients(out var mockFromSystemUsage);
            SetupSystemUsageRepository(systemUsages);
            SetupContractRepository();

            // Act
            var result = _sut.ModifyRelation(FromSystemUsageId, FromSystemRelationId, ToSystemUsageId, Interface2Id, Contract2Id);

            // Assert
            Assert.True(result.Ok);

            Assert.Equal(Contract2Id, GetModifiedSystemRelation(mockFromSystemUsage).AssociatedContract.Id);
        }

        #region Helpers

        private Dictionary<int, ItSystemUsage> SetupSystemRelationWithIngredients(out Mock<ItSystemUsage> mockFromSystemUsage)
        {
            mockFromSystemUsage = SetupMockSystemUsage(FromSystemUsageId);
            var mockToSystemUsage = SetupMockSystemUsage(ToSystemUsageId);
            var fromSystemUsage = mockFromSystemUsage.Object;
            var systemUsages = new Dictionary<int, ItSystemUsage>
                {{FromSystemUsageId, fromSystemUsage}, {ToSystemUsageId, mockToSystemUsage.Object}};
            SetupSystemRelation(FromSystemRelationId, mockFromSystemUsage, mockToSystemUsage);
            _mockAuthorizationContext.Setup(c => c.AllowModify(fromSystemUsage)).Returns(true);
            return systemUsages;
        }

        private static Mock<ItSystemUsage> SetupMockSystemUsage(int id)
        {
            var mockSystemUsage1 = new Mock<ItSystemUsage>();
            mockSystemUsage1.Object.Id = id;
            return mockSystemUsage1;
        }

        private static void SetupSystemRelation(int systemRelationId, Mock<ItSystemUsage> sourceSystemUsage, Mock<ItSystemUsage> targetSystemUsage)
        {
            var usageSystemRelation = new SystemRelation(sourceSystemUsage.Object)
            {
                Id = systemRelationId,
                RelationInterface = new ItInterface { Id = Interface1Id },
                AssociatedContract = new ItContract { Id = Contract1Id }
            };
            usageSystemRelation.SetRelationTo(targetSystemUsage.Object);
            sourceSystemUsage.SetupGet(u => u.UsageRelations).Returns(new List<SystemRelation> {usageSystemRelation});
            var itInterfaceExhibits = new List<ItInterfaceExhibit>
            {
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = Interface1Id }},
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = Interface2Id }}
            };
            var mockTargetItSystem = new Mock<ItSystem>();
            mockTargetItSystem.SetupGet(s => s.ItInterfaceExhibits).Returns(itInterfaceExhibits);
            targetSystemUsage.SetupGet(u => u.ItSystem).Returns(mockTargetItSystem.Object);
        }

        private void SetupSystemUsageRepository(IReadOnlyDictionary<int, ItSystemUsage> systemUsages)
        {
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<object[]>()))
                .Returns((object[] key) => systemUsages[(int) key[0]]);
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

        #endregion
    }
}
