using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices.Queries.DataProcessingRegistrations;
using Tests.Toolkit.Patterns;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.DPR
{
    //Data processing registration query
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
            var mainContract = new ItContract
            {
                Terminated = _now.Date.AddDays(-1)
            };

            //Arrange
            var excludedSinceReadModelIsCurrentlyInactive = DataProcessingRegistrationReadModelTestData.CreateReadModel(false, null);
            var includedSinceContractIsExpired = DataProcessingRegistrationReadModelTestData.CreateReadModel(true, mainContract);


            var input = new[]
            {
                excludedSinceReadModelIsCurrentlyInactive,
                includedSinceContractIsExpired
            };

            //Act
            var result = _sut.Apply(input.AsQueryable()).ToList();

            //Assert
            var includedResult = Assert.Single(result);
            Assert.Same(includedSinceContractIsExpired, includedResult);
        }
    }
}
