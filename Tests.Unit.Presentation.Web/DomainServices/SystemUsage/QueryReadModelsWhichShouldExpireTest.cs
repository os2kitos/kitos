using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices.Queries.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;
using static Tests.Unit.Presentation.Web.Helpers.ItSystemUsageOverviewReadModelTestData;

namespace Tests.Unit.Presentation.Web.DomainServices.SystemUsage
{
    //System usage query
    public class QueryReadModelsWhichShouldExpireTest : WithAutoFixture
    {
        private readonly DateTime _now;
        private readonly QueryReadModelsWhichShouldExpire _sut;

        public QueryReadModelsWhichShouldExpireTest()
        {
            _now = DateTime.UtcNow.Date;
            _sut = new QueryReadModelsWhichShouldExpire(_now);
        }

        [Fact]
        public void Apply_Includes_Systems_Which_Are_Currently_Active_But_Should_Be_Inactive()
        {
            var mainContract = new ItContractItSystemUsage
            {
                ItContract = new ItContract
                {
                    Terminated = _now.Date.AddDays(-1)
                }
            };

            //Arrange
            var excludedSinceReadModelIsCurrentlyInActive = CreateReadModel(false, _now.AddDays(-2), _now.AddDays(-1), null);
            var excludedBecauseExpirationDateIsNull = CreateReadModel(true, _now.AddDays(-2), null, null);
            var excludedBecauseExpirationDateHasNotPassed = CreateReadModel(true, _now.AddDays(-2), _now, null);
            var includedSinceExpiredAndContractTerminated = CreateReadModel(true, null, _now.AddDays(-1), mainContract);


            var input = new[]
            {
                excludedSinceReadModelIsCurrentlyInActive,
                excludedBecauseExpirationDateIsNull,
                excludedBecauseExpirationDateHasNotPassed,
                includedSinceExpiredAndContractTerminated
            };

            //Act
            var result = _sut.Apply(input.AsQueryable()).ToList();

            //Assert
            var includedResult = Assert.Single(result);
            Assert.Same(includedSinceExpiredAndContractTerminated, includedResult);
        }
    }
}
