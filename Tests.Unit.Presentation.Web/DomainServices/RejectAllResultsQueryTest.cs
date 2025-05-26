using System.Linq;
using AutoFixture;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class RejectAllResultsQueryTest : WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            fixture.Register(() => new ItSystem { Id = fixture.Create<int>() });
            base.OnFixtureCreated(fixture);
        }

        [Fact]
        public void Apply_Removes_All_Results_From_Input()
        {
            //Arrange
            var itSystems = Many<ItSystem>().AsQueryable();


            //Act
            var result = new RejectAllResultsQuery<ItSystem>().Apply(itSystems);

            //Assert
            Assert.Empty(result);
        }
    }
}
