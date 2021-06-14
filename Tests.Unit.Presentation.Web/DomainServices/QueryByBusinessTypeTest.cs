using System;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries.ItSystem;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByBusinessTypeTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Uuid_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new ItSystem { BusinessType = new BusinessType { Uuid = correctId } };
            var excludedNoBusinessType = new ItSystem { BusinessType = null };
            var excludedWrongUuid = new ItSystem { BusinessType = new BusinessType { Uuid = incorrectId } };

            var input = new[] { excludedWrongUuid, matched, excludedNoBusinessType }.AsQueryable();
            var sut = new QueryByBusinessType(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }
    }
}
