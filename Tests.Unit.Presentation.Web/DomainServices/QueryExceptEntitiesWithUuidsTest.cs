using System;
using AutoFixture;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryExceptEntitiesWithUuidsTest : WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            fixture.Register(() => new ItSystem { Uuid = fixture.Create<Guid>() });
            base.OnFixtureCreated(fixture);
        }

        [Fact]
        public void Apply_Removes_Entities_With_Id_Match()
        {
            //Arrange
            var excludedSystems = Many<ItSystem>().ToList();
            var includedSystems = Many<ItSystem>().ToList();
            var sut = new QueryExceptEntitiesWithUuids<ItSystem>(excludedSystems.Select(x => x.Uuid));
            var input = excludedSystems.Concat(includedSystems).AsQueryable();

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.True(includedSystems.SequenceEqual(result));
        }
    }
}
