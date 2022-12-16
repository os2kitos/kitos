using Core.DomainModel.ItContract;
using System;
using System.Linq;
using Core.DomainServices.Queries.DataProcessingRegistrations;
using Tests.Toolkit.Patterns;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.DPR
{
    //Data processing registration query
    public class QueryReadModelsWhichShouldBecomeActiveTest : WithAutoFixture
    {

        private readonly DateTime _now;
        private readonly QueryReadModelsWhichShouldBecomeActive _sut;

        public QueryReadModelsWhichShouldBecomeActiveTest()
        {
            _now = DateTime.UtcNow.Date;
            _sut = new QueryReadModelsWhichShouldBecomeActive(_now);
        }

        [Fact]
        public void Apply_Includes_Systems_Which_Are_Currently_Inactive_But_Should_Be_Active()
        {
            var inactiveMainContract =  new ItContract
            {
                ExpirationDate = _now.Date.AddDays(-1)
            };
            var activeMainContract = new ItContract
            {
                Active = true
            };

            //Arrange
            var excludedSinceReadModelIsCurrentlyActive = DataProcessingRegistrationReadModelTestData.CreateReadModel(true, null);
            var excludedSinceMainContractIsNotActive = DataProcessingRegistrationReadModelTestData.CreateReadModel(true, inactiveMainContract);
            var includedSinceMainContractIsNull= DataProcessingRegistrationReadModelTestData.CreateReadModel(false, null);
            var includedSinceMainContractIsActive = DataProcessingRegistrationReadModelTestData.CreateReadModel(false, activeMainContract);

            var input = new[]
            {
                excludedSinceReadModelIsCurrentlyActive,
                excludedSinceMainContractIsNotActive,
                includedSinceMainContractIsNull,
                includedSinceMainContractIsActive,
            };

            //Act
            var result = _sut.Apply(input.AsQueryable()).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(includedSinceMainContractIsNull, result);
            Assert.Contains(includedSinceMainContractIsActive, result);
        }
    }
}
