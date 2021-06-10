using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.External.V2.Response;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.V2
{
    public static class OrganizationV2Helper
    {
        public static async Task<IEnumerable<OrganizationResponseDTO>> GetOrganizationsForWhichUserIsRightsHolder(Cookie loginCookie, int page = 0, int pageSize = 10)
        {
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/organizations/with-rightsholder-access?page={page}&pageSize={pageSize}"), loginCookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosV2ApiResponseAsync<IEnumerable<OrganizationResponseDTO>>();
        }
    }
}
