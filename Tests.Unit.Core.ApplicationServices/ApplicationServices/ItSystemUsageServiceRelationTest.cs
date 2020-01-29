using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
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
        private readonly Mock<IGenericRepository<ItSystemUsage>> _mockSystemUsageRepository;
        private readonly Mock<IAuthorizationContext> _mockAuthorizationContext;
        private readonly Mock<IItSystemRepository> _mockSystemRepository;
        private readonly Mock<IItContractRepository> _mockContractRepository;
        private readonly Mock<IOptionsService<SystemRelation, RelationFrequencyType>> _mockOptionsService;
        private readonly Mock<IOrganizationalUserContext> _mockOrganizationalUserContext;

        public ItSystemUsageServiceRelationTest()
        {
            _mockSystemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _mockAuthorizationContext = new Mock<IAuthorizationContext>();
            _mockSystemRepository = new Mock<IItSystemRepository>();
            _mockContractRepository = new Mock<IItContractRepository>();
            _mockOptionsService = new Mock<IOptionsService<SystemRelation, RelationFrequencyType>>();
            _mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        private void ModifyRelation_GivenAnyUserWithItSystemWriteAccess_AllowsModifications(bool allowModifications)
        {
            // Arrange
            const int systemUsageId = 1;
            var itSystemUsage = new ItSystemUsage { Id = systemUsageId };
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<int>())).Returns(itSystemUsage);
            _mockAuthorizationContext.Setup(c => c.AllowModify(itSystemUsage)).Returns(allowModifications);

            var sut = new ItSystemUsageService(
                _mockSystemUsageRepository.Object,
                _mockAuthorizationContext.Object,
                _mockSystemRepository.Object,
                _mockContractRepository.Object,
                _mockOptionsService.Object,
                _mockOrganizationalUserContext.Object);

            // Act
            var result = sut.ModifyRelation(systemUsageId, 0, 0);

            // Assert
            Assert.True(allowModifications ? result.Ok : result.Error.FailureType == OperationFailure.Forbidden);
        }

        [Fact]
        private void ModifyRelation_GivenOneSystemUsingAnotherAndReplacementSystem_SecondSystemIsReplaced()
        {
            // Arrange
            var mockSystemUsage1 = SetupMockSystemUsage(1);
            var mockSystemUsage2 = SetupMockSystemUsage(2);
            var mockSystemUsage3 = SetupMockSystemUsage(3);
            var systemUsages = new Dictionary<int, ItSystemUsage> {{ 1, mockSystemUsage1.Object}, { 2, mockSystemUsage2.Object}, { 3, mockSystemUsage3.Object}};
            SetupUsageSystemRelation(11, mockSystemUsage1, mockSystemUsage2);
            SetupUsedBySystemRelation(12, mockSystemUsage2, mockSystemUsage1);
            _mockSystemUsageRepository.Setup(r => r.GetByKey(It.IsAny<object[]>()))
                .Returns((object[] key) => systemUsages[(int)key[0]]);
            _mockAuthorizationContext.Setup(c => c.AllowModify(mockSystemUsage1.Object)).Returns(true);
            _mockOrganizationalUserContext.SetupGet(c => c.UserEntity).Returns(new User());

            var sut = new ItSystemUsageService(
                _mockSystemUsageRepository.Object,
                _mockAuthorizationContext.Object,
                _mockSystemRepository.Object,
                _mockContractRepository.Object,
                _mockOptionsService.Object,
                _mockOrganizationalUserContext.Object);

            // Act
            var result = sut.ModifyRelation(1, 11, 3);

            // Assert
            Assert.True(result.Ok);
            var modifiedSystemRelation = mockSystemUsage1.Object.UsageRelations.First(r => r.Id == 11);
            Assert.Equal(mockSystemUsage3.Object, modifiedSystemRelation.RelationTarget);
        }

        #region Helpers

        private static Mock<ItSystemUsage> SetupMockSystemUsage(int id)
        {
            var mockSystemUsage1 = new Mock<ItSystemUsage>();
            mockSystemUsage1.Object.Id = id;
            return mockSystemUsage1;
        }

        private static void SetupUsageSystemRelation(int id, Mock<ItSystemUsage> sourceSystemUsage, IMock<ItSystemUsage> deswtinationSystemUsage)
        {
            var usageSystemRelation = new SystemRelation(sourceSystemUsage.Object, deswtinationSystemUsage.Object)
            {
                Id = id
            };
            sourceSystemUsage.Object.UsageRelations = new List<SystemRelation> {usageSystemRelation};
        }

        private static void SetupUsedBySystemRelation(int id, Mock<ItSystemUsage> sourceSystemUsage, IMock<ItSystemUsage> destinationSystemUsage)
        {
            var usedBySystemRelation = new SystemRelation(sourceSystemUsage.Object, destinationSystemUsage.Object)
            {
                Id = id
            };
            sourceSystemUsage.Object.UsedByRelations = new List<SystemRelation> {usedBySystemRelation};
        }

        #endregion
    }
}
