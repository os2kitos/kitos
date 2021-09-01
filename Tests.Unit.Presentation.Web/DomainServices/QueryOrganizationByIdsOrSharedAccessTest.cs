using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryOrganizationByIdsOrSharedAccessTest : WithAutoFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Apply_Includes_All_Results_With_Either_Id_Match_Or_Public_Access(bool includeShared)
        {
            //Arrange
            var includedOrganization1 = A<int>();
            var includedOrganization2 = includedOrganization1 + 1;
            var includedShared = includedOrganization2 + 1;
            var excluded = includedShared + 1;

            var sut = new QueryOrganizationByIdsOrSharedAccess(new[] { includedOrganization1, includedOrganization2 }, includeShared);

            var includedWithOrgMatch1 = CreateOrganization(includedOrganization1, AccessModifier.Local);
            var includedWithOrgMatch2 = CreateOrganization(includedOrganization2, AccessModifier.Public);
            var includedWithAccessMatch = CreateOrganization(includedShared, AccessModifier.Public);
            var excludedLocalSystemInOtherOrg = CreateOrganization(excluded, AccessModifier.Local);

            var input = new[]
            {
                includedWithOrgMatch1,
                excludedLocalSystemInOtherOrg,
                includedWithOrgMatch2,
                includedWithAccessMatch,
            }.AsQueryable();

            //Act
            var organizations = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(includeShared ? 3 : 2, organizations.Count);
            if (includeShared)
            {
                Assert.Contains(organizations, x => x == includedWithAccessMatch);
            }
            Assert.Contains(organizations, x => x == includedWithOrgMatch1);
            Assert.Contains(organizations, x => x == includedWithOrgMatch2);

        }

        private static Organization CreateOrganization(int organizationId, AccessModifier accessModifier)
        {
            return new()
            {
                Id = organizationId,
                AccessModifier = accessModifier
            };
        }
    }
}
