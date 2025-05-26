using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainServices.Queries.Notifications;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Advices
{
    public class QueryByOwnerResourceIdTest : WithAutoFixture
    {
        
        [Fact]
        public void Apply_Returns_Items_With_RelationId()
        {
            //Arrange
            var validId = A<int>();
            var included = new Advice { RelationId = validId};
            var excludedIncorrectId = new Advice { RelationId = A<int>()};

            var input = new[] { included, excludedIncorrectId }.AsQueryable();
            var sut = new QueryByOwnerResourceId(validId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var notification = Assert.Single(result);
            Assert.Same(included, notification);
        }
    }
}
