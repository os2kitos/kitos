using System;
using System.Linq;
using Core.DomainModel.ItContract;
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
            var inactiveMainContract = new ItContractItSystemUsage
            {
                ItContract = new ItContract
                {
                    ExpirationDate = _now.Date.AddDays(-1)
                }
            };
            var activeMainContract = new ItContractItSystemUsage
            {
                ItContract = new ItContract
                {
                    Active = true
                }
            };

            //Arrange - 1 that must expire and 1 that must be activated
            var includedSinceConcludedHasPassedAndNotExpired = CreateReadModel(false, _now.Date, _now.Date.AddDays(1), null);
            var excludedSinceConcludedSinceExpired = CreateReadModel(false, _now.Date.AddDays(-2), _now.Date.AddDays(-1), null);
            var excludedBecauseExpirationDateHasNotPassed = CreateReadModel(true, _now.AddDays(-2), _now, null);
            var excludedSinceTerminated = CreateReadModel(false, null, null, inactiveMainContract);
            var includedSinceMainContractIsActive= CreateReadModel(false, null, null, activeMainContract);

            _repositoryMock.Setup(x => x.AsQueryable()).Returns(new[]
            {
                excludedSinceTerminated,
                includedSinceConcludedHasPassedAndNotExpired,
                excludedSinceConcludedSinceExpired,
                excludedBecauseExpirationDateHasNotPassed,
                includedSinceMainContractIsActive
            }.AsQueryable());

            //Act
            var result = _sut.GetReadModelsMustUpdateToChangeActiveState().ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(includedSinceConcludedHasPassedAndNotExpired, result);
            Assert.Contains(includedSinceMainContractIsActive, result);
        }
    }
}
