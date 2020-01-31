using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
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
        private const int SourceSystemUsageId = 1;
        private const int TargetSystemUsageId = 2;
        private const int SourceSystemRelationId = 11;

        private readonly Mock<IGenericRepository<ItSystemUsage>> _mockSystemUsageRepository;
        private readonly Mock<IAuthorizationContext> _mockAuthorizationContext;
        private readonly Mock<IItSystemRepository> _mockSystemRepository;
        private readonly Mock<IItContractRepository> _mockContractRepository;
        private readonly Mock<IOptionsService<SystemRelation, RelationFrequencyType>> _mockOptionsService;
        private readonly Mock<IOrganizationalUserContext> _mockOrganizationalUserContext;
        private readonly Mock<IGenericRepository<SystemRelation>> _mockSystemRelationRepository;
        private readonly Mock<ITransactionManager> _mockTransactionManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<ItInterface> _mockSourceSystemInterface;
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
            _mockSourceSystemInterface = new Mock<ItInterface>();
            _mockSourceSystemInterface.Object.Id = 21;
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
            var systemUsage = new ItSystemUsage { Id = SourceSystemUsageId };
            var systemRelation = new SystemRelation(systemUsage) { Id = SourceSystemRelationId };
            systemUsage.UsageRelations = new List<SystemRelation> { systemRelation };
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<int>())).Returns(systemUsage);
            _mockAuthorizationContext.Setup(c => c.AllowModify(systemUsage)).Returns(allowModifications);

            // Act
            var result = _sut.ModifyRelation(SourceSystemUsageId, SourceSystemRelationId,A<int>());

            // Assert
            Assert.True(allowModifications ? result.Ok : result.Error.FailureType == OperationFailure.Forbidden);
        }

        [Fact]
        private void ModifyRelation_GivenSourceSystemUsingTarget_WhenTargetIsReplaced_RelationIsUpdated()
        {
            // Arrange
            const int replacementSystemUsageId = 3;
            var mockSourceSystemUsage = SetupMockSystemUsage(SourceSystemUsageId);
            var mockTargetSystemUsage = SetupMockSystemUsage(TargetSystemUsageId);
            var mockReplacementSystemUsage = SetupMockSystemUsage(replacementSystemUsageId);
            var systemUsages = new Dictionary<int, ItSystemUsage> {{ SourceSystemUsageId, mockSourceSystemUsage.Object}, { TargetSystemUsageId, mockTargetSystemUsage.Object}, { replacementSystemUsageId, mockReplacementSystemUsage.Object}};
            SetupUsageSystemRelation(SourceSystemRelationId, mockSourceSystemUsage, mockTargetSystemUsage);
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<object[]>()))
                .Returns((object[] key) => systemUsages[(int)key[0]]);
            _mockAuthorizationContext.Setup(c => c.AllowModify(mockSourceSystemUsage.Object)).Returns(true);

            // Act
            var result = _sut.ModifyRelation(SourceSystemUsageId, SourceSystemRelationId, replacementSystemUsageId, null);

            // Assert
            Assert.True(result.Ok);
            var modifiedSystemRelation = mockSourceSystemUsage.Object.UsageRelations.First(r => r.Id == SourceSystemRelationId);
            Assert.Equal(mockReplacementSystemUsage.Object, modifiedSystemRelation.ToSystemUsage);
            Assert.Null(modifiedSystemRelation.RelationInterface);
        }

        [Fact]
        private void ModifyRelation_GivenSystemUsageAndInterfaceChange_ValidatesInterfaceIsExposedByTargetSystemAndUpdates()
        {
            // Arrange
            var mockSourceSystemUsage = SetupMockSystemUsage(SourceSystemUsageId);
            var mockTargetSystemUsage = SetupMockSystemUsage(TargetSystemUsageId);
            var systemUsages = new Dictionary<int, ItSystemUsage> {{ SourceSystemUsageId, mockSourceSystemUsage.Object}, { TargetSystemUsageId, mockTargetSystemUsage.Object}};            
            SetupUsageSystemRelation(SourceSystemRelationId, mockSourceSystemUsage, mockTargetSystemUsage);
            _mockOrganizationalUserContext.SetupGet(c => c.UserEntity).Returns(new User());
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<object[]>()))
                .Returns((object[] key) => systemUsages[(int)key[0]]);
            _mockAuthorizationContext.Setup(c => c.AllowModify(mockSourceSystemUsage.Object)).Returns(true);

            // Act
            var result = _sut.ModifyRelation(SourceSystemUsageId, SourceSystemRelationId, TargetSystemUsageId, 100);

            // Assert
            Assert.True(result.Ok);
            var modifiedSystemRelation = mockSourceSystemUsage.Object.UsageRelations.First(r => r.Id == SourceSystemRelationId);
            Assert.Equal(100, modifiedSystemRelation.RelationInterface.Id);
        }

        #region Helpers

        private static Mock<ItSystemUsage> SetupMockSystemUsage(int id)
        {
            var mockSystemUsage1 = new Mock<ItSystemUsage>();
            mockSystemUsage1.Object.Id = id;
            return mockSystemUsage1;
        }

        private void SetupUsageSystemRelation(int systemRelationId, Mock<ItSystemUsage> sourceSystemUsage, Mock<ItSystemUsage> targetSystemUsage)
        {
            var usageSystemRelation = new SystemRelation(sourceSystemUsage.Object)
            {
                Id = systemRelationId,
                RelationInterface = _mockSourceSystemInterface.Object
            };
            usageSystemRelation.SetRelationTo(targetSystemUsage.Object);
            sourceSystemUsage.SetupGet(u => u.UsageRelations).Returns(new List<SystemRelation> {usageSystemRelation});
            var itInterfaceExhibits = new List<ItInterfaceExhibit>
            {
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = 100 }},
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = 101 }}
            };
            var mockTargetItSystem = new Mock<ItSystem>();
            mockTargetItSystem.SetupGet(s => s.ItInterfaceExhibits).Returns(itInterfaceExhibits);
            targetSystemUsage.SetupGet(u => u.ItSystem).Returns(mockTargetItSystem.Object);
        }

        #endregion
    }
}
