using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainServices.Queries.Notifications;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Advices
{
    public class QueryByActiveAdviceTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Active_Items()
        {
            //Arrange
            var included = new Advice {IsActive = true};
            var excluded = new Advice {IsActive = false};

            var input = new[] { included, excluded }.AsQueryable();
            var sut = new QueryByActiveAdvice(true);

            //Act
            var result = sut.Apply(input);

            //Assert
            var notification = Assert.Single(result);
            Assert.Same(included, notification);
        }
    }
}
