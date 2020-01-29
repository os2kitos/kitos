﻿using System.Collections.Generic;
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
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class ItSystemUsageServiceRelationTest
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
        private readonly Mock<ItInterface> _mockSourceSystemInterface;

        public ItSystemUsageServiceRelationTest()
        {
            _mockSystemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _mockAuthorizationContext = new Mock<IAuthorizationContext>();
            _mockSystemRepository = new Mock<IItSystemRepository>();
            _mockContractRepository = new Mock<IItContractRepository>();
            _mockOptionsService = new Mock<IOptionsService<SystemRelation, RelationFrequencyType>>();
            _mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
            _mockSourceSystemInterface = new Mock<ItInterface>();
            _mockSourceSystemInterface.Object.Id = 21;
            _mockOrganizationalUserContext.SetupGet(c => c.UserEntity).Returns(new User());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        private void ModifyRelation_GivenAnyUserWithItSystemWriteAccess_AllowsModifications(bool allowModifications)
        {
            // Arrange
            var systemUsage = new ItSystemUsage { Id = SourceSystemUsageId };
            var systemRelation = new SystemRelation(systemUsage, systemUsage) { Id = SourceSystemRelationId };
            systemUsage.UsageRelations = new List<SystemRelation> { systemRelation };
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<int>())).Returns(systemUsage);
            _mockAuthorizationContext.Setup(c => c.AllowModify(systemUsage)).Returns(allowModifications);

            var sut = new ItSystemUsageService(
                _mockSystemUsageRepository.Object,
                _mockAuthorizationContext.Object,
                _mockSystemRepository.Object,
                _mockContractRepository.Object,
                _mockOptionsService.Object,
                _mockOrganizationalUserContext.Object);

            // Act
            var result = sut.ModifyRelation(SourceSystemUsageId, SourceSystemRelationId);

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

            var sut = new ItSystemUsageService(
                _mockSystemUsageRepository.Object,
                _mockAuthorizationContext.Object,
                _mockSystemRepository.Object,
                _mockContractRepository.Object,
                _mockOptionsService.Object,
                _mockOrganizationalUserContext.Object);

            // Act
            var result = sut.ModifyRelation(SourceSystemUsageId, SourceSystemRelationId, replacementSystemUsageId, null);

            // Assert
            Assert.True(result.Ok);
            var modifiedSystemRelation = mockSourceSystemUsage.Object.UsageRelations.First(r => r.Id == SourceSystemRelationId);
            Assert.Equal(mockReplacementSystemUsage.Object, modifiedSystemRelation.RelationTarget);
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

            var sut = new ItSystemUsageService(
                _mockSystemUsageRepository.Object,
                _mockAuthorizationContext.Object,
                _mockSystemRepository.Object,
                _mockContractRepository.Object,
                _mockOptionsService.Object,
                _mockOrganizationalUserContext.Object);

            // Act
            var result = sut.ModifyRelation(SourceSystemUsageId, SourceSystemRelationId, TargetSystemUsageId, 100);

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

        private void SetupUsageSystemRelation(int systemRelationId, IMock<ItSystemUsage> sourceSystemUsage, Mock<ItSystemUsage> targetSystemUsage = null)
        {
            var usageSystemRelation = new SystemRelation(sourceSystemUsage.Object, targetSystemUsage.Object)
            {
                Id = systemRelationId,
                RelationInterface = _mockSourceSystemInterface.Object
            };
            sourceSystemUsage.Object.UsageRelations = new List<SystemRelation> {usageSystemRelation};
            var itInterfaceExhibits = new List<ItInterfaceExhibit>
            {
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = 100 }},
                new ItInterfaceExhibit { ItInterface = new ItInterface { Id = 101 }}
            };
            var mockTargetItSystem = new Mock<ItSystem>();
            mockTargetItSystem.SetupGet(s => s.ItInterfaceExhibits).Returns(itInterfaceExhibits);
            targetSystemUsage?.SetupGet(u => u.ItSystem).Returns(mockTargetItSystem.Object);
        }

        #endregion
    }
}
