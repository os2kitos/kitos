using Core.DomainModel.Advice;
using Core.DomainModel.Shared;
using Core.DomainServices.Queries.Notifications;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Advices
{
    public class QueryByOwnerResourceTest : WithAutoFixture
    {
        [Theory]
        [InlineData(RelatedEntityType.itSystemUsage, RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itSystemUsage, RelatedEntityType.itContract)]
        [InlineData(RelatedEntityType.dataProcessingRegistration, RelatedEntityType.itSystemUsage)]
        [InlineData(RelatedEntityType.dataProcessingRegistration, RelatedEntityType.itContract)]
        [InlineData(RelatedEntityType.itContract, RelatedEntityType.dataProcessingRegistration)]
        [InlineData(RelatedEntityType.itContract, RelatedEntityType.itSystemUsage)]
        public void Apply_Returns_Items_With_RelationId(RelatedEntityType validType, RelatedEntityType invalidType)
        {
            //Arrange
            var included = new Advice { Type = validType };
            var excludedIncorrectType = new Advice { Type = invalidType };

            var input = new[] { included, excludedIncorrectType }.AsQueryable();
            var sut = new QueryByOwnerResource(validType);

            //Act
            var result = sut.Apply(input);

            //Assert
            var notification = Assert.Single(result);
            Assert.Same(included, notification);
        }
    }
}
