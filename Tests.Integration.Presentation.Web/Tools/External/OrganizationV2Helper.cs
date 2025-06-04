using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
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

        public static async Task<IEnumerable<OrganizationResponseDTO>> GetOrganizationsAsync(string token, int page = 0, int pageSize = 10, string nameContent = null, bool onlyWhereUserHasMembership = false, string cvrContent = null, string nameOrCvrContent = null, Guid? uuid = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            if (nameContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContent", nameContent));

            if (onlyWhereUserHasMembership)
                queryParameters.Add(new KeyValuePair<string, string>("onlyWhereUserHasMembership", true.ToString()));

            if (cvrContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("cvrContent", cvrContent));

            if (nameOrCvrContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameOrCvrContent", nameOrCvrContent));

            if (uuid != null)
                queryParameters.Add(new KeyValuePair<string, string>("uuid", uuid.Value.ToString("D")));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            var url = TestEnvironment.CreateUrl($"api/v2/organizations?{query}");
            using var response = await HttpApi.GetWithTokenAsync(url, token);
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

        public static async Task<OrganizationPermissionsResponseDTO> GetPermissionsAsync(Cookie cookie, Guid uuid)
        {
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/organizations/{uuid:D}/permissions"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<OrganizationPermissionsResponseDTO>();
        }

        public static async Task<UnitAccessRightsResponseDTO> GetUnitAccessRights(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{organizationUuid}/organization-units/{unitUuid}/permissions");

            using var response = await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UnitAccessRightsResponseDTO>();
        }

        public static async Task<List<UnitAccessRightsWithUnitDataResponseDTO>> GetUnitAccessRightsForOrganization(Guid organizationUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{organizationUuid}/organization-units/all/collection-permissions");

            using var response = await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
            var res = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<List<UnitAccessRightsWithUnitDataResponseDTO>>();
        }

        public static async Task<List<OrganizationUserResponseDTO>> GetUsersInOrganization(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/organizations/{organizationUuid}/users");
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<List<OrganizationUserResponseDTO>>();
        }
    }
}
