using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByCvrContentTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Includes_Organizations_With_Match_On_CvrContent()
        {
            //Arrange
            var matchContent = 8738;
            var match1 = new Organization { Cvr = $"9{matchContent}933" };
            var match2 = new Organization { Cvr = $"DK-1{matchContent}933" };
            var unMatched = new Organization { Cvr = $"DK-9{matchContent + 1}933" };
            var input = new[] { match1, unMatched, match2 }.AsQueryable();
            var sut = new QueryByCvrContent(matchContent.ToString());

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x == match1);
            Assert.Contains(result, x => x == match2);
        }
    }
}
