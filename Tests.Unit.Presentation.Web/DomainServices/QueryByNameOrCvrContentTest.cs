using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByNameOrCvrContentTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Includes_Organizations_With_Match_On_Name_Or_Cvr_Content()
        {
            //Arrange
            var matchContent = A<int>() % 99999999;
            var unmatchedContent = matchContent + 1;
            var matchName = new Organization { Cvr = $"{unmatchedContent}", Name = $"name-{matchContent}" };
            var matchCvr = new Organization { Cvr = $"DK-1{matchContent}933", Name = $"{unmatchedContent}" };
            var unMatched = new Organization { Cvr = $"{unmatchedContent}", Name = $"{unmatchedContent}" };
            var input = new[] { matchName, unMatched, matchCvr }.AsQueryable();
            var sut = new QueryByNameOrCvrContent(matchContent.ToString());

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x == matchName);
            Assert.Contains(result, x => x == matchCvr);
        }
    }
}
