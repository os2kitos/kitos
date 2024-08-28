using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    internal class OrganizationGridConfigurationTestHelper
    {

        private static string createPath(Guid orgUuid, string operation)
        {
            return $"api/v2/internal/organizations/{orgUuid:D}/grid-configuration/{operation}?overviewType=0";
        }

        public static async Task<HttpResponseMessage> SendGetConfigurationRequestAsync(Guid orgUuid,
            OrganizationRole orgRole = OrganizationRole.LocalAdmin)
        {

            var url = TestEnvironment.CreateUrl(createPath(orgUuid, "get"));
            var cookie = await HttpApi.GetCookieAsync(orgRole);
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        public static async Task<HttpResponseMessage> SendSaveConfigurationRequestAsync(Guid orgUuid, IEnumerable<KendoColumnConfigurationDTO> columns,
            OrganizationRole orgRole = OrganizationRole.LocalAdmin)
        {
            var url = TestEnvironment.CreateUrl(createPath(orgUuid, "save"));
            var body = new OrganizationGridConfigurationRequestDTO { OrganizationUuid = orgUuid, VisibleColumns = columns, OverviewType = 0};
            var cookie = await HttpApi.GetCookieAsync(orgRole);
            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }
    }
}
