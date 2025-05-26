using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByOrganizationIdTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Removes_Items_Without_OrganizationIdMatch()
        {
            //Arrange
            var organizationId = A<int>();

            var included1 = CreateItSystem(organizationId);
            var included2 = CreateItSystem(organizationId);
            var excluded = CreateItSystem(A<int>());
            var input = new[] { included1, included2, excluded }.AsQueryable();
            var sut = new QueryByOrganizationId<ItSystem>(organizationId);

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(included1, result);
            Assert.Contains(included2, result);

        }

        private ItSystem CreateItSystem(int organizationId)
        {
            return new ItSystem
            {
                Id = A<int>(),
                OrganizationId = organizationId
            };
        }
    }
}
