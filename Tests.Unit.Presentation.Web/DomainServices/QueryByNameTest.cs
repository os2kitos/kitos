using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByNameTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Results_Where_Name_Is_Found()
        {
            //Arrange
            var correctName = A<string>();
            var excluded1 = CreateItSystem(correctName + A<string>());
            var excluded2 = CreateItSystem(A<string>() + correctName + A<string>());
            var excluded3 = CreateItSystem(A<string>() + correctName);
            var included1 = CreateItSystem(correctName);
            var included2 = CreateItSystem(correctName);
            var input = new[] { excluded1, excluded2, excluded3, included1 , included2}.AsQueryable();
            var sut = new QueryByName<ItSystem>(correctName);

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(included1, result);
            Assert.Contains(included2, result);
        }

        private ItSystem CreateItSystem(string name)
        {
            return new ItSystem
            {
                Id = A<int>(),
                Name = name
            };
        }
    }
}
