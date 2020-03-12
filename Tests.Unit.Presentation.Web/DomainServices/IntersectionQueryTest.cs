using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class IntersectionQueryTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_The_Intersection_Of_All_Provided_Queries()
        {
            //Arrange
            var organizationId = A<int>();
            var subQuery1 = new QueryByAccessModifier<ItSystem>(AccessModifier.Local);
            var subQuery2 = new QueryByOrganizationId<ItSystem>(organizationId);

            var included = CreateItSystem(organizationId, AccessModifier.Local);
            var excluded1 = CreateItSystem(organizationId, AccessModifier.Public);
            var excluded2 = CreateItSystem(A<int>(), AccessModifier.Local);

            var input = new[] { included, excluded1, excluded2 }.AsQueryable();

            var sut = new IntersectionQuery<ItSystem>(new List<IDomainQuery<ItSystem>>() { subQuery1, subQuery2 });

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(included, itSystem);
        }

        private ItSystem CreateItSystem(int organizationId, AccessModifier accessModifier)
        {
            return new ItSystem()
            {
                Id = A<int>(),
                OrganizationId = organizationId,
                AccessModifier = accessModifier
            };
        }
    }
}
