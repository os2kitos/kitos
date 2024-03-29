﻿using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices.Queries.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;
using static Tests.Unit.Presentation.Web.Helpers.ItSystemUsageOverviewReadModelTestData;

namespace Tests.Unit.Presentation.Web.DomainServices.SystemUsage
{
    public class QueryReadModelsWhichShouldBecomeActiveTest : WithAutoFixture
    {
        private readonly DateTime _now;
        private readonly QueryReadModelsWhichShouldBecomeActive _sut;

        public QueryReadModelsWhichShouldBecomeActiveTest()
        {
            this._now = DateTime.UtcNow.Date;
            _sut = new QueryReadModelsWhichShouldBecomeActive(_now);
        }

        [Fact]
        public void Apply_Includes_Systems_Which_Are_Currently_Inactive_But_Should_Be_Active()
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

            //Arrange
            var excludedSinceReadModelIsCurrentlyActive = CreateReadModel(true, null, null, null);
            var excludedSinceConcludedHasNotYetPassed = CreateReadModel(false, _now.Date.AddDays(1), null, inactiveMainContract);
            var includedSinceConcludedHasPassedAndNoExpiration = CreateReadModel(false, _now.Date, null, null);
            var includedSinceConcludedHasPassedAndNotExpired = CreateReadModel(false, _now.Date, _now.Date.AddDays(1), null);
            var includedSinceMainContractIsActive= CreateReadModel(false, null, null, activeMainContract);
            var excludedSinceConcludedSinceExpired = CreateReadModel(false, _now.Date.AddDays(-2), _now.Date.AddDays(-1), inactiveMainContract);
            var excludedSinceConcludedSinceContractExpired = CreateReadModel(false, _now.Date.AddDays(2), null, inactiveMainContract);

            var input = new[]
            {
                excludedSinceReadModelIsCurrentlyActive,
                excludedSinceConcludedHasNotYetPassed,
                includedSinceConcludedHasPassedAndNoExpiration,
                includedSinceConcludedHasPassedAndNotExpired,
                includedSinceMainContractIsActive,
                excludedSinceConcludedSinceExpired,
                excludedSinceConcludedSinceContractExpired
            };

            //Act
            var result = _sut.Apply(input.AsQueryable()).ToList();

            //Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(includedSinceConcludedHasPassedAndNoExpiration, result);
            Assert.Contains(includedSinceConcludedHasPassedAndNotExpired, result);
            Assert.Contains(includedSinceMainContractIsActive, result);
        }
    }
}
