using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.Shared;
using Core.DomainServices.Queries.Notifications;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Advices
{
    public class QueryByOwnerResourceIdTest : WithAutoFixture
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
            var validId = A<int>();
            var included = new Advice { RelationId = validId, Type = validType };
            var excludedBothIncorrect = new Advice { RelationId = A<int>(), Type = invalidType};
            var excludedIncorrectId = new Advice { RelationId = validId, Type = invalidType};
            var excludedIncorrectType = new Advice { RelationId = A<int>(), Type = validType};

            var input = new[] { included, excludedBothIncorrect, excludedIncorrectId, excludedIncorrectType }.AsQueryable();
            var sut = new QueryByOwnerResourceId(validId, validType);

            //Act
            var result = sut.Apply(input);

            //Assert
            var notification = Assert.Single(result);
            Assert.Same(included, notification);
        }
    }
}
