using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models.API.V2.Request.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationInternalApiV2Test: WithAutoFixture
    {
        private const int CvrMaxLength = 10;

        [Fact]
        public async Task CanPatchOrganizationMasterDataWithValues()
        {
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var patchDto = new OrganizationMasterDataRequestDTO
            {
                Address = A<string>(),
                Cvr = Truncate(A<string>(), CvrMaxLength),
                Email = A<string>(),
                Phone = A<string>()
            };

            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250);
            var organizationToPatch = organizations.First();
            Assert.NotNull(organizationToPatch);

            var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterData(organizationToPatch.Uuid, patchDto);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(organizationToPatch.Uuid.ToString(), content);
            Assert.Contains(patchDto.Cvr, content);
        }

        [Fact]
        public async Task CanPatchOrganizationMasterDataWithNull()
        {
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var patchDto = new OrganizationMasterDataRequestDTO();

            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250);
            var organizationToPatch = organizations.First();
            Assert.NotNull(organizationToPatch);

            var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterData(organizationToPatch.Uuid, patchDto);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(organizationToPatch.Uuid.ToString(), content);
            Assert.Contains("null", content);
        }

        private static string Truncate(string s, int limit)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= limit ? s : s.Substring(0, limit);
        }
    }
}
