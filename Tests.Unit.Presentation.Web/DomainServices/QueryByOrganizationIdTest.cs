using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Unit.Presentation.Web.Helpers;
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
            Assert.True(result.Contains(included1));
            Assert.True(result.Contains(included2));

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
