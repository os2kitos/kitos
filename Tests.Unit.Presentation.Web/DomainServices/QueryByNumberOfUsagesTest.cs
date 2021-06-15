using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.ItSystem;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByNumberOfUsagesTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_MoreThanOrEqualToUsages()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            const int gtEq = 2;
            var matchedOnEq = new ItSystem { Usages = { new ItSystemUsage(), new ItSystemUsage() } };
            var matchedOnGte = new ItSystem { Usages = { new ItSystemUsage(), new ItSystemUsage(), new ItSystemUsage() } };
            var lessThanLowerBound = new ItSystem { Usages = new List<ItSystemUsage> { new() } };

            var input = new[] { matchedOnEq, lessThanLowerBound, matchedOnGte }.AsQueryable();
            var sut = new QueryByNumberOfUsages(gtEq);

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2,result.Count);
            Assert.Equal(new []{matchedOnEq,matchedOnGte},result.ToArray());
        }
    }
}
