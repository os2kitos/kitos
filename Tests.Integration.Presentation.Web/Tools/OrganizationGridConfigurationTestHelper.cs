using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    internal class OrganizationGridConfigurationTestHelper
    {

        private static string CreatePath(Guid orgUuid, string operation, OverviewType overviewType = OverviewType.ItSystemUsage)
        {
            return $"api/v2/internal/organizations/{orgUuid:D}/grid-configuration/{overviewType}/{operation}";
        }

        public static async Task<HttpResponseMessage> SendGetConfigurationRequestAsync(Guid orgUuid,
            Cookie cookie = null)
        {

            var url = TestEnvironment.CreateUrl(CreatePath(orgUuid, "get"));
            var httpCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.GetWithCookieAsync(url, httpCookie);
        }

        public static async Task<HttpResponseMessage> SendSaveConfigurationRequestAsync(Guid orgUuid, IEnumerable<KendoColumnConfigurationDTO> columns,
            Cookie cookie = null)
        {
            var url = TestEnvironment.CreateUrl(CreatePath(orgUuid, "save"));
            var body = new OrganizationGridConfigurationRequestDTO { OrganizationUuid = orgUuid, VisibleColumns = columns, OverviewType = 0};
            var httpCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.PostWithCookieAsync(url, httpCookie, body);
        }

        public static async Task<OrganizationGridConfigurationResponseDTO> SaveConfigurationRequestAsync(Guid orgUuid,
            IEnumerable<KendoColumnConfigurationDTO> columns,
            Cookie cookie = null)
        {
            var response = await SendSaveConfigurationRequestAsync(orgUuid, columns, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OrganizationGridConfigurationResponseDTO>();

        }

        public static async Task<HttpResponseMessage> SendDeleteConfigurationRequestAsync(Guid orgUuid,
            Cookie cookie = null)
        {
            var url = TestEnvironment.CreateUrl(CreatePath(orgUuid, "delete"));
            var httpCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.DeleteWithCookieAsync(url, httpCookie);
        }

        public static async Task<OrganizationGridConfigurationResponseDTO> GetResponseBodyAsync(Guid orgUuid, Cookie cookie = null)
        {
            using var response = await SendGetConfigurationRequestAsync(orgUuid, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OrganizationGridConfigurationResponseDTO>();
        }
    }
}
