using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByPublicAccessOrOrganizationIdTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Includes_All_Results_With_Either_Org_Id_Match_Or_Public_Access()
        {
            //Arrange
            var organizationId = A<int>();
            var otherOrganizationId = A<int>();
            var sut = new QueryByPublicAccessOrOrganizationId<ItSystem>(organizationId);

            var includedWithOrgMatch = CreateItSystem(organizationId, AccessModifier.Local);
            var includedWithAccessMatch = CreateItSystem(otherOrganizationId, AccessModifier.Public);

            var input = new[]
            {
                includedWithOrgMatch,
                CreateItSystem(otherOrganizationId,AccessModifier.Local),
                includedWithAccessMatch,
            }.AsQueryable();

            //Act
            var systems = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, systems.Count);
            Assert.Contains(includedWithOrgMatch, systems);
            Assert.Contains(includedWithAccessMatch, systems);
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
