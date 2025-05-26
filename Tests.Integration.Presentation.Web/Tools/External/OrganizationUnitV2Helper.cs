using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class OrganizationUnitV2Helper
    {
        public static async Task<IEnumerable<OrganizationUnitResponseDTO>> GetOrganizationUnitsAsync(string token, Guid organizationUuid, int page = 0, int pageSize = 10, string nameQuery = null)
        {
            using var response = await SendGetOrganizationUnitsAsync(token, organizationUuid, page, pageSize, nameQuery);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<OrganizationUnitResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetOrganizationUnitsAsync(string token, Guid organizationUuid, int page = 0, int pageSize = 10, string nameQuery = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            if (nameQuery != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameQuery", nameQuery));

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
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/organizations/{organizationUuid}/organization-units/{organizationUnitId:D}"), token);
        }

        public static async Task<OrganizationUnitResponseDTO> CreateUnitAsync(Guid organizationUuid,
            CreateOrganizationUnitRequestDTO request, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v2/internal/organizations/{organizationUuid}/organization-units/create"), requestCookie, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<OrganizationUnitResponseDTO>();
        }

        public static async Task<OrganizationUnitResponseDTO> PatchUnitAsync(Guid organizationUuid, Guid unitUuid,
            UpdateOrganizationUnitRequestDTO request, Cookie cookie = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v2/internal/organizations/{organizationUuid}/organization-units/{unitUuid}/patch"), requestCookie, request);
            Assert.Equal(expectedStatusCode, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<OrganizationUnitResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendDeleteUnitAsync(Guid organizationUuid, Guid unitUuid, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl(
                $"api/v2/internal/organizations/{organizationUuid}/organization-units/{unitUuid}/delete");
            return await HttpApi.DeleteWithCookieAsync(url, requestCookie);
        }

        public static async Task<IEnumerable<OrganizationUnitRolesResponseDTO>> GetUnitRolesAsync(Guid organizationUuid, Guid unitUuid,
            Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl(RolesRoutePrefix(organizationUuid, unitUuid));
            using var response = await HttpApi.GetWithCookieAsync(url, requestCookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<OrganizationUnitRolesResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendCreateRoleAssignmentAsync(Guid organizationUuid, Guid unitUuid, CreateOrganizationUnitRoleAssignmentRequestDTO request,
            Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{RolesRoutePrefix(organizationUuid, unitUuid)}/create");
            return await HttpApi.PostWithCookieAsync(url, requestCookie, request);
        }

        public static async Task<HttpResponseMessage> SendCreateBulkRoleAssignmentAsync(Guid organizationUuid, Guid unitUuid, BulkRoleAssignmentRequestDTO request,
            Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{RolesRoutePrefix(organizationUuid, unitUuid)}/bulk/create");
            return await HttpApi.PostWithCookieAsync(url, requestCookie, request);
        }

        public static async Task<HttpResponseMessage> SendDeleteRoleAssignmentAsync(Guid organizationUuid, Guid unitUuid, DeleteOrganizationUnitRoleAssignmentRequestDTO request,
            Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{RolesRoutePrefix(organizationUuid, unitUuid)}/delete");
            return await HttpApi.DeleteWithCookieAsync(url, requestCookie, request);
        }

        public static async Task<OrganizationUnitResponseDTO> GetRootUnit(Guid organizationUuid)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var units = await GetOrganizationUnitsAsync(token.Token, organizationUuid);
            return units.First(x => x.ParentOrganizationUnit == null);
        }

        private static string RolesRoutePrefix(Guid organizationUuid, Guid unitUuid)
        {
            return $"{ControllerRoutePrefix(organizationUuid)}/{unitUuid}/roles";
        }

        private static string ControllerRoutePrefix(Guid organizationUuid)
        {
            return $"api/v2/internal/organizations/{organizationUuid}/organization-units";
        }
    }
}
