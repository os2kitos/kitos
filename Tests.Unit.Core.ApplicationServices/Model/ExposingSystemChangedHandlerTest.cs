using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http.Description;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Handlers;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Infrastructure.Services.DataAccess;
using Moq;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class ExposingSystemChangedHandlerTest
    {
        [Fact]
        private void
            Handle_GivenInterfaceAndPossibleSystemChange_WhenSystemInUse_ThenInterfaceOnSystemRelationsIsReset()
        {
            // Arrange
            var mockChangedSystem = new Mock<ItSystem>();
            var mockFirstFromSystemUsage = new Mock<ItSystemUsage>();
            var mockFirstToSystemUsage = new Mock<ItSystemUsage>();
            mockFirstToSystemUsage.SetupGet(su => su.ItSystem).Returns(mockChangedSystem.Object);
            var mockSecondFromSystemUsage = new Mock<ItSystemUsage>();
            var mockSecondToSystemUsage = new Mock<ItSystemUsage>();
            mockSecondToSystemUsage.SetupGet(su => su.ItSystem).Returns(mockChangedSystem.Object);
            var mockAffectedInterface = new Mock<ItInterface>();
            var systemRelations = new List<SystemRelation>
            {
                SetupSystemRelation(1, mockFirstFromSystemUsage, mockAffectedInterface, mockFirstToSystemUsage),
                SetupSystemRelation(2, mockSecondFromSystemUsage, mockAffectedInterface, mockSecondToSystemUsage),
            };
            mockAffectedInterface.SetupGet(i => i.AssociatedSystemRelations).Returns(systemRelations);
            var mockSystemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            var itSystemUsages = new List<ItSystemUsage>
            {
                mockFirstFromSystemUsage.Object,
                mockSecondFromSystemUsage.Object
            };
            mockSystemUsageRepository
                .Setup(r => r.GetWithReferencePreload(su => su.UsageRelations))
                .Returns(itSystemUsages.AsQueryable());
            var mockTransactionManager = new Mock<ITransactionManager>();
            var mockTransaction = new Mock<IDatabaseTransaction>();
            mockTransactionManager.Setup(m => m.Begin(It.IsAny<IsolationLevel>())).Returns(mockTransaction.Object);
            var mockUserContext = new Mock<IOrganizationalUserContext>();
            mockUserContext.SetupGet(uc => uc.UserEntity).Returns(new User());
            var mockClock = new Mock<IOperationClock>();

            // Act
            var sut = new ExposingSystemChangedHandler(mockSystemUsageRepository.Object, mockTransactionManager.Object,
                mockUserContext.Object, mockClock.Object);
            sut.Handle(new ExposingSystemChanged(mockAffectedInterface.Object));

            // Assert
            // TODO: Check mockFirst/SecondSystemUsage relation removals
        }

        #region Helpers

        private static SystemRelation SetupSystemRelation(int id, Mock<ItSystemUsage> mockFirstFromSystemUsage,
            IMock<ItInterface> mockAffectedInterface, IMock<ItSystemUsage> mockFirstToSystemUsage)
        {
            var systemRelation = new SystemRelation(mockFirstFromSystemUsage.Object)
            {
                Id = id,
                RelationInterface = mockAffectedInterface.Object,
                ToSystemUsage = mockFirstToSystemUsage.Object
            };
            mockFirstFromSystemUsage.SetupGet(su => su.UsageRelations)
                .Returns(new List<SystemRelation> {systemRelation});
            return systemRelation;
        }

        #endregion
    }
}
