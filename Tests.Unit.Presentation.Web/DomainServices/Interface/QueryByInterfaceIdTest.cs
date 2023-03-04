using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries.Interface;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Interface
{
    public class QueryByInterfaceIdTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Includes_Results_With_InterfaceId()
        {
            //Arrange
            var interfaceId = A<string>();

            var valid = CreateInterface(interfaceId);
            var invalid = CreateInterface($"{interfaceId}1");

            var input = new[] {valid, invalid}.AsQueryable();
            var sut = new QueryByInterfaceId(interfaceId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itInterface = Assert.Single(result);
            Assert.Equal(valid.ItInterfaceId, itInterface.ItInterfaceId);
        }

        private static ItInterface CreateInterface(string interfaceId)
        {
            return new ItInterface { ItInterfaceId = interfaceId};
        }
    }
}
