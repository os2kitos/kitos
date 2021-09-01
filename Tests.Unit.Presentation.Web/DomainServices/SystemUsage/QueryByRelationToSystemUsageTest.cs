using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.SystemUsage
{
    public class QueryByRelationToSystemUsageTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = CreateWithRelationTo(correctId);
            var excluded = CreateWithRelationTo(incorrectId);

            var input = new[] { matched, excluded }.AsQueryable();
            var sut = new QueryByRelationToSystemUsage(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var entity = Assert.Single(result);
            Assert.Same(matched, entity);
        }

        private static ItSystemUsage CreateWithRelationTo(Guid uuid)
        {
            return new() { UsageRelations = new List<SystemRelation> {new(new ItSystemUsage()){ToSystemUsage = new ItSystemUsage {Uuid = uuid}}}};
        }
    }
}
