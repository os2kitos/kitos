using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.KLE;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.KLE
{
    public class QueryByDescriptionContentTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_With_Prefix_Match()
        {
            //Arrange
            var content = A<string>();
            var match1 = new TaskRef { Description = $"{content}{A<string>()}" };
            var noMatch = new TaskRef { Description = $"{content.Substring(0,content.Length-1)}"};
            var match2 = new TaskRef { Description = $"{A<string>()}{content}" };

            var input = new[] { match1, noMatch, match2 }.AsQueryable();

            var sut = new QueryByDescriptionContent(content);

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(new[] { match1, match2 }, result.ToArray());
        }
    }
}
