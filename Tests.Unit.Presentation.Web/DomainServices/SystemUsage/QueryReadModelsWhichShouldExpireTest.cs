using System;
using System.Linq;
using Core.DomainServices.Queries.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;
using static Tests.Unit.Presentation.Web.Helpers.ItSystemUsageOverviewReadModelTestData;

namespace Tests.Unit.Presentation.Web.DomainServices.SystemUsage
{
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
        public void Apply_Includes_Systems_Which_Are_Currently_Inactive_But_Should_Be_Active()
        {
            //Arrange
            var excludedSinceReadModelIsCurrentlyInActive = CreateReadModel(false, _now.AddDays(-2), _now.AddDays(-1));
            var excludedBecauseExpirationDateIsNull = CreateReadModel(true, _now.AddDays(-2), null);
            var excludedBecauseExpirationDateHasNotPassed = CreateReadModel(true, _now.AddDays(-2), _now);
            var includedSinceExpired = CreateReadModel(true, null, _now.AddDays(-1));


            var input = new[]
            {
                excludedSinceReadModelIsCurrentlyInActive,
                excludedBecauseExpirationDateIsNull,
                excludedBecauseExpirationDateHasNotPassed,
                includedSinceExpired
            };

            //Act
            var result = _sut.Apply(input.AsQueryable()).ToList();

            //Assert
            var includedResult = Assert.Single(result);
            Assert.Same(includedSinceExpired, includedResult);
        }
    }
}
