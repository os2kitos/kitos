using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Authorization
{
    public class GetOrganizationsTests : WithAutoFixture
    {
        [Theory]
        [InlineData("Name")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Get_Organizations_Returns_List(string orderBy)
        {
            var response = await OrganizationHelper.GetOrganizationsResponseAsync(orderBy);

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Get_Organizations_Does_Not_Allow_For_Property_Other_Than_Name()
        {
            var incorrectOrderBy = "IncorrectPropertyName";

            var response = await OrganizationHelper.GetOrganizationsResponseAsync(incorrectOrderBy);

            Assert.True(!response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Get_Organizations_Returns_List_Sorted_Ascending_By_Name()
        {
            var orderBy = "Name";
            var organization1 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "OrgTest" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var organization2 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "TestOrg1" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var organization3 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "TestOrg2" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);

            var result = await OrganizationHelper.GetOrganizationsAsync(orderBy);
            var indexOfOrg1 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization1.Id));
            var indexOfOrg2 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization2.Id));
            var indexOfOrg3 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization3.Id));

            Assert.True(indexOfOrg1 < indexOfOrg2);
            Assert.True(indexOfOrg2 < indexOfOrg3);
        }

        [Fact]
        public async Task Get_Organizations_Returns_List_Sorted_Descending_By_Name()
        {
            var orderBy = "Name";
            var orderByAsc = false;
            var organization1 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "OrgTest" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var organization2 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "TestOrg1" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var organization3 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "TestOrg2" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);

            var result = await OrganizationHelper.GetOrganizationsAsync(orderBy, orderByAsc);
            var indexOfOrg1 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization1.Id));
            var indexOfOrg2 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization2.Id));
            var indexOfOrg3 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization3.Id));

            Assert.True(indexOfOrg1 > indexOfOrg2);
            Assert.True(indexOfOrg2 > indexOfOrg3);
        }
    }
}
