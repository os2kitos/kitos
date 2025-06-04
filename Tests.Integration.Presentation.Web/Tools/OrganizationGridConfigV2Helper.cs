using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class OrganizationGridConfigV2Helper
    {

        private static string GetPathForGridConfigOperations(Guid orgUuid, string operation, OverviewType overviewType = OverviewType.ItSystemUsage)
        {
            return $"{GetControllerPath(orgUuid)}/{overviewType}/{operation}";
        }

        private static string GetControllerPath(Guid orgUuid)
        {
            return $"api/v2/internal/organizations/{orgUuid:D}/grid";
        }

        private static string GetPermissionsPath(Guid orgUuid)
        {
            return $"{GetControllerPath(orgUuid)}/permissions";
        }

        public static async Task<HttpResponseMessage> SendGetConfigurationRequestAsync(Guid orgUuid,
            Cookie cookie = null)
        {

            var url = TestEnvironment.CreateUrl(GetPathForGridConfigOperations(orgUuid, "get"));
            var httpCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.GetWithCookieAsync(url, httpCookie);
        }

        public static async Task<HttpResponseMessage> SendSaveConfigurationRequestAsync(Guid orgUuid, IEnumerable<ColumnConfigurationRequestDTO> columns,
            Cookie cookie = null)
        {
            var url = TestEnvironment.CreateUrl(GetPathForGridConfigOperations(orgUuid, "save"));
            var body = new OrganizationGridConfigurationRequestDTO { VisibleColumns = columns};
            var httpCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.PostWithCookieAsync(url, httpCookie, body);
        }

        public static async Task<OrganizationGridConfigurationResponseDTO> SaveConfigurationRequestAsync(Guid orgUuid,
            IEnumerable<ColumnConfigurationRequestDTO> columns,
            Cookie cookie = null)
        {
            var response = await SendSaveConfigurationRequestAsync(orgUuid, columns, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OrganizationGridConfigurationResponseDTO>();

        }

        public static async Task<HttpResponseMessage> SendDeleteConfigurationRequestAsync(Guid orgUuid,
            Cookie cookie = null)
        {
            var url = TestEnvironment.CreateUrl(GetPathForGridConfigOperations(orgUuid, "delete"));
            var httpCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.DeleteWithCookieAsync(url, httpCookie);
        }

        public static async Task<OrganizationGridConfigurationResponseDTO> GetResponseBodyAsync(Guid orgUuid, Cookie cookie = null)
        {
            using var response = await SendGetConfigurationRequestAsync(orgUuid, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OrganizationGridConfigurationResponseDTO>();
        }

        public static async Task<OrganizationGridPermissionsResponseDTO> GetOrganizationPermissionsAsync(Guid orgUuid, Cookie cookie = null)
        {
            var url = TestEnvironment.CreateUrl(GetPermissionsPath(orgUuid));
            var httpCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(url, httpCookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OrganizationGridPermissionsResponseDTO>();
        }
    }
}
