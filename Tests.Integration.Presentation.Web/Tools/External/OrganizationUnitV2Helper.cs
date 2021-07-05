using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.External.V2.Response.Organization;
using Presentation.Web.Models.External.V2.Types;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class OrganizationUnitV2Helper
    {
        public static async Task<IEnumerable<OrganizationUnitResponseDTO>> GetOrganizationUnitsAsync(string token, Guid organizationUuid, int page = 0, int pageSize = 10, string nameQuery = null, OrganizationUserRole? roleQuery = null)
        {
            using var response = await SendGetOrganizationUnitsAsync(token, organizationUuid, page, pageSize, nameQuery, roleQuery);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<OrganizationUnitResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetOrganizationUnitsAsync(string token, Guid organizationUuid, int page = 0, int pageSize = 10, string nameQuery = null, OrganizationUserRole? roleQuery = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new("page", page.ToString("D")),
                new("namequery", pageSize.ToString("D")),
            };

            if (nameQuery != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameQuery", nameQuery));

            if (roleQuery.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("roleQuery", roleQuery.Value.ToString("G")));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/organizations/{organizationUuid}/organization-units?{query}"), token);
        }


        public static async Task<OrganizationUnitResponseDTO> GetOrganizationUnitAsync(string token, Guid organizationUuid, Guid organizationUnitId)
        {
            using var response = await SendGetOrganizationUnitAsync(token, organizationUuid, organizationUnitId);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<OrganizationUnitResponseDTO>();
        }
        public static async Task<HttpResponseMessage> SendGetOrganizationUnitAsync(string token, Guid organizationUuid, Guid organizationUnitId)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/organizations/{organizationUuid}/users/{organizationUnitId:D}"), token);
        }
    }
}
