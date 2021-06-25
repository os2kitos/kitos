using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.External.V2.Response;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class OrganizationV2Helper
    {
        public static async Task<IEnumerable<OrganizationResponseDTO>> GetOrganizationsForWhichUserIsRightsHolder(string token, int page = 0, int pageSize = 10)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/rightsholder/organizations?page={page}&pageSize={pageSize}"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<OrganizationResponseDTO>>();
        }
    }
}
