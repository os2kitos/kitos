using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries.Interface;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Interface
{
    public class QueryByNameOrItInterfaceIdTest: WithAutoFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Apply_Includes_Results_With_Name_Or_ItInterfaceId_Match(bool testName)
        {
            //Arrange
            var interfaceId = "testInterfaceId";
            var name = "testName";
            var query = testName ? name.Substring(0, 4) : interfaceId.Substring(0, 4);

            var valid = testName ? CreateInterface(A<string>(), name) : CreateInterface(interfaceId, A<string>());
            var invalid = CreateInterface(A<string>(), A<string>());

            var input = new[] { valid, invalid }.AsQueryable();
            var sut = new QueryByNameOrItInterfaceId(query);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itInterface = Assert.Single(result);
            Assert.Equal(valid.ItInterfaceId, itInterface.ItInterfaceId);
        }

        private static ItInterface CreateInterface(string interfaceId, string name)
        {
            return new ItInterface { ItInterfaceId = interfaceId, Name = name };
        }
    }
}
