using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByAccessModifierTest : WithAutoFixture
    {
        [Theory]
        [InlineData(AccessModifier.Public, AccessModifier.Local)]
        [InlineData(AccessModifier.Local, AccessModifier.Public)]
        public void Apply_Includes_Results_With_AccessModifierMatch(AccessModifier includedModifier, AccessModifier excludedModifier)
        {
            //Arrange
            var included1 = CreateSystem(includedModifier);
            var included2 = CreateSystem(includedModifier);
            var excluded = CreateSystem(excludedModifier);
            var input = new[] { included1, excluded, included2 }.AsQueryable();
            var sut = new QueryByAccessModifier<ItSystem>(includedModifier);

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(included1, result);
            Assert.Contains(included2, result);
        }

        private ItSystem CreateSystem(AccessModifier accessModifier)
        {
            return new ItSystem
            {
                Id = A<int>(),
                AccessModifier = accessModifier
            };
        }
    }
}
