using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class KendoOverviewConfigurationHelper
    {
        public static async Task<HttpResponseMessage> SendSaveConfigurationRequestAsync(int orgId, OverviewType overviewType, IEnumerable<KendoColumnConfigurationDTO> columns, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/kendo-organizational-configuration");

            var body = new KendoOrganizationalConfigurationDTO
            {
                OverviewType = overviewType,
                OrganizationId = orgId,
                Columns = columns
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<HttpResponseMessage> SendGetConfigurationRequestAsync(int orgId, OverviewType overviewType, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/kendo-organizational-configuration?organizationId={orgId}&overviewType={overviewType}");

            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        public static async Task<HttpResponseMessage> SendDeleteConfigurationRequestAsync(int orgId, OverviewType overviewType, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/kendo-organizational-configuration?organizationId={orgId}&overviewType={overviewType}");

            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }
    }
}
