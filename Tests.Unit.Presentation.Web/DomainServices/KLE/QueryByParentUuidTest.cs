using System;
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.KLE;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.KLE
{
    public class QueryByParentUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_With_Parent_Match()
        {
            //Arrange
            var parentUuid = A<Guid>();
            var parent = new TaskRef { Uuid = parentUuid };
            var match1 = new TaskRef { Parent = parent };
            var notMatchedWrongParent = new TaskRef { Parent = new TaskRef() };
            var notMatchedNoParentParent = new TaskRef();
            var match2 = new TaskRef { Parent = parent };

            var input = new[] { match1, notMatchedNoParentParent, notMatchedWrongParent, match2, parent }.AsQueryable();

            var sut = new QueryByParentUuid(parentUuid);

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(new[] { match1, match2 }, result.ToArray());
        }
    }
}
