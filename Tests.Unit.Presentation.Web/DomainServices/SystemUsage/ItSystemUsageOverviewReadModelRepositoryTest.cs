using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;
using static Tests.Unit.Presentation.Web.Helpers.ItSystemUsageOverviewReadModelTestData;

namespace Tests.Unit.Presentation.Web.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelRepositoryTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewReadModel>> _repositoryMock;
        private readonly ItSystemUsageOverviewReadModelRepository _sut;
        private readonly DateTime _now;

        public ItSystemUsageOverviewReadModelRepositoryTest()
        {
            _repositoryMock = new Mock<IGenericRepository<ItSystemUsageOverviewReadModel>>();
            _sut = new ItSystemUsageOverviewReadModelRepository(_repositoryMock.Object);
            _now = DateTime.Now;
        }

        [Fact]
        public void GetReadModelsMustUpdateToChangeActiveState_Returns_ReadModels_Where_Active_State_Is_Stale()
        {
            //Arrange - 1 that must expire and 1 that must be activated
            var includedSinceConcludedHasPassedAndNotExpired = CreateReadModel(false, _now.Date, _now.Date.AddDays(1));
            var excludedSinceConcludedSinceExpired = CreateReadModel(false, _now.Date.AddDays(-2), _now.Date.AddDays(-1));
            var excludedBecauseExpirationDateHasNotPassed = CreateReadModel(true, _now.AddDays(-2), _now);
            var includedSinceExpired = CreateReadModel(true, null, _now.AddDays(-1));
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(new[]
            {
                includedSinceExpired,
                includedSinceConcludedHasPassedAndNotExpired,
                excludedSinceConcludedSinceExpired,
                excludedBecauseExpirationDateHasNotPassed
            }.AsQueryable());

            //Act
            var result = _sut.GetReadModelsMustUpdateToChangeActiveState().ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(includedSinceExpired, result);
            Assert.Contains(includedSinceConcludedHasPassedAndNotExpired, result);
        }
    }
}
