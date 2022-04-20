using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        [InlineData(true)]
        [InlineData(false)]
        public async Task Get_Organizations_Returns_List_Sorted_By_Name(bool orderByAsc)
        {
            var orderBy = "Name";
            var organization1 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "OrgTest" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var organization2 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "TestOrg1" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var organization3 = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "TestOrg2" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);

            using var response = await OrganizationHelper.SendGetOrganizationsRequestAsync(orderBy, orderByAsc);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.ReadResponseBodyAsKitosApiResponseAsync<List<Organization>>();
            var indexOfOrg1 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization1.Id));
            var indexOfOrg2 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization2.Id));
            var indexOfOrg3 = result.IndexOf(result.FirstOrDefault(prp => prp.Id == organization3.Id));

            if (orderByAsc)
            {
                Assert.True(indexOfOrg1 < indexOfOrg2);
                Assert.True(indexOfOrg2 < indexOfOrg3);
                return;
            }

            Assert.True(indexOfOrg1 > indexOfOrg2);
            Assert.True(indexOfOrg2 > indexOfOrg3);
        }

        [Fact]
        public async Task Get_Organizations_Does_Not_Allow_For_Property_Other_Than_Name()
        {
            var incorrectOrderBy = "IncorrectPropertyName";

            using var response = await OrganizationHelper.SendGetOrganizationsRequestAsync(incorrectOrderBy);
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Get_Organizations_Returns_List_When_Name_Parameter_Is_Empty(string orderBy)
        {
            await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "OrgTest" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, "TestOrg1" + Guid.NewGuid(), "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);

            using var response = await OrganizationHelper.SendGetOrganizationsRequestAsync(orderBy);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.ReadResponseBodyAsKitosApiResponseAsync<List<Organization>>();

            Assert.True(result.Count >= 2);
        }
    }
}
