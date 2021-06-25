using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByEnabledEntitiesOnlyTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Removes_Deactivated_Entities()
        {
            //Arrange
            var excludedSystems = CreateListOfItSystems(true, A<int>());
            var includedSystems = CreateListOfItSystems(false, A<int>());
            var sut = new QueryByEnabledEntitiesOnly<ItSystem>();
            var input = excludedSystems.Concat(includedSystems).AsQueryable();

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.True(includedSystems.SequenceEqual(result));
        }

        private List<ItSystem> CreateListOfItSystems(bool shouldBeDeactivated, int numberOfSystems)
        {
            var systems = new List<ItSystem>();
            for (int i = 0; i < numberOfSystems; i++)
            {
                systems.Add(new ItSystem() { Disabled = shouldBeDeactivated });
            }
            return systems;
        }
    }
}
