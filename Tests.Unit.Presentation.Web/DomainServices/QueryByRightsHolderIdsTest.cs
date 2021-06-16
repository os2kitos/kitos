using System.Linq;
using AutoFixture;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries.ItSystem;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByRightsHolderIdsTest : WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            fixture.Register(() => new ItSystem { Id = fixture.Create<int>() });
            base.OnFixtureCreated(fixture);
        }

        [Fact]
        public void Apply_Removes_Entities_Without_Id_Match()
        {
            //Arrange
            var correctRightsHolderId = A<int>();
            var wrongRightsHolderId = correctRightsHolderId + 1;

            var excludedSystemsNoRightsHolder = Many<ItSystem>().ToList();

            var excludedSystemsWrongRightsHolder = Many<ItSystem>().ToList();
            excludedSystemsWrongRightsHolder.ForEach(x => x.BelongsToId = wrongRightsHolderId);

            var includedSystems = Many<ItSystem>().ToList();
            includedSystems.ForEach(x => x.BelongsToId = correctRightsHolderId);

            var sut = new QueryByRightsHolderIds(new[] { correctRightsHolderId });
            var input = excludedSystemsNoRightsHolder.Concat(excludedSystemsWrongRightsHolder).Concat(includedSystems).AsQueryable();

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.True(includedSystems.SequenceEqual(result));
        }
    }
}
