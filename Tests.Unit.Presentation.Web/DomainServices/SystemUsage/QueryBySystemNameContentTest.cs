using System;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.SystemUsage
{
    public class QueryBySystemNameContentTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var content = A<string>();
            var matchedContent = content;
            var matchedContentPartially = $"{A<string>()}{content}{A<string>()}";

            var incorrectId = A<Guid>();
            var match1 = new ItSystemUsage { ItSystem = new ItSystem { Name = matchedContent } };
            var match2 = new ItSystemUsage { ItSystem = new ItSystem { Name = matchedContentPartially } };
            var excluded = new ItSystemUsage { ItSystem = new ItSystem { Name = matchedContent.Substring(0, matchedContent.Length - 1) } };

            var input = new[] { match1, excluded, match2 }.AsQueryable();
            var sut = new QueryBySystemNameContent(content);

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x == match1);
            Assert.Contains(result, x => x == match2);
        }
    }
}
