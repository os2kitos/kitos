using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.External.V2.Response.Organization;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class OrganizationV2Helper
    {
        public static async Task<IEnumerable<ShallowOrganizationResponseDTO>> GetOrganizationsForWhichUserIsRightsHolder(string token, int page = 0, int pageSize = 10)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/rightsholder/organizations?page={page}&pageSize={pageSize}"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<OrganizationResponseDTO>>();
        }

        public static async Task<IEnumerable<OrganizationResponseDTO>> GetOrganizationsAsync(string token, int page = 0, int pageSize = 10)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/organizations?page={page}&pageSize={pageSize}"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<OrganizationResponseDTO>>();
        }

        public static async Task<OrganizationResponseDTO> GetOrganizationAsync(string token, Guid uuid)
        {
            var response = await SendGetOrganizationAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<OrganizationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetOrganizationAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/organizations/{uuid:D}"), token);
        }
    }
}
