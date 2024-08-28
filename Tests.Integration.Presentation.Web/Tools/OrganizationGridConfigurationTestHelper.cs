using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;

namespace Tests.Integration.Presentation.Web.Tools
{
    internal class OrganizationGridConfigurationTestHelper
    {
        public static async Task<HttpResponseMessage> SendSaveConfigurationRequestAsync(Guid uuid, OverviewType overviewType, IEnumerable<KendoColumnConfigurationDTO> columns, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{uuid}/grid-configuration/save?overviewType={overviewType}");
            var body = columns;
            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<HttpResponseMessage> SendGetConfigurationRequestAsync(Guid uuid, OverviewType overviewType, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{uuid}/grid-configuration/get?overviewType={overviewType}");

            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        public static async Task<HttpResponseMessage> SendDeleteConfigurationRequestAsync(Guid uuid, OverviewType overviewType, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{uuid}/grid-configuration/delete?overviewType={overviewType}");

            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }
    }
}
