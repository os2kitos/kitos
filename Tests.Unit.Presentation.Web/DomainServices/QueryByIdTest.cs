using System.Linq;
using AutoFixture;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByIdTest : WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            fixture.Register(() => new ItSystem { Id = fixture.Create<int>() });
            base.OnFixtureCreated(fixture);
        }

        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var matchedSystem = A<ItSystem>();
            var excludedSystem = A<ItSystem>();

            var input = new[] { matchedSystem, excludedSystem }.AsQueryable();
            var sut = new QueryById<ItSystem>(matchedSystem.Id);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matchedSystem, itSystem);
        }
    }
}
