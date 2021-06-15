using System;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.ItSystem;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByRightsHolderTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Uuid_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new ItSystem { BelongsTo = new Organization() { Uuid = correctId } };
            var excludedNoRightsHolder = new ItSystem { BelongsTo = null };
            var excludedWrongUuid = new ItSystem { BelongsTo = new Organization { Uuid = incorrectId } };

            var input = new[] { excludedWrongUuid, matched, excludedNoRightsHolder }.AsQueryable();
            var sut = new QueryByRightsHolderUuid(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }
    }
}
