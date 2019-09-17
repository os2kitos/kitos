using System.Linq;
using AutoFixture;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByIdsTest : WithAutoFixture
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
            var excludedSystems = Many<ItSystem>().ToList();
            var includedSystems = Many<ItSystem>().ToList();
            var sut = new QueryByIds<ItSystem>(includedSystems.Select(x => x.Id));
            var input = excludedSystems.Concat(includedSystems).AsQueryable();

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.True(includedSystems.SequenceEqual(result));
        }

        [Fact]
        public void Apply_Removes_All_If_No_Ids_Provided()
        {
            //Arrange
            var sut = new QueryByIds<ItSystem>(Enumerable.Empty<int>());
            var input = Many<ItSystem>().AsQueryable();

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Empty(result);
        }
    }
}
