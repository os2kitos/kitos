using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.KLE;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.KLE
{
    public class QueryByKeyPrefixTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_With_Prefix_Match()
        {
            //Arrange
            var prefix = "00.0";
            var match1 = new TaskRef { TaskKey = $"{prefix}{A<uint>() % 10}" };
            var notMatchedOnGroup = new TaskRef { TaskKey = "00.10" };
            var notMatchedOnMainGroup = new TaskRef { TaskKey = "01.00" };
            var match2 = new TaskRef { TaskKey = $"{prefix}0.{A<uint>() % 100}" };

            var input = new[] { match1, notMatchedOnMainGroup, notMatchedOnGroup, match2 }.AsQueryable();

            var sut = new QueryByKeyPrefix(prefix);

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(new[] { match1, match2 }, result.ToArray());
        }
    }
}
