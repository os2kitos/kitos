using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1.GDPR;
using Presentation.Web.Models.API.V1.UI_Configuration;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.UI_Configuration
{
    public class UIConfigurationHelper
    {

        public static async Task<HttpResponseMessage> SendPutRequestAsync(int organizationId, string module, List<CustomizedUINodeDTO> body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var uri = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}");
            return await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie, body);
        }

        public static async Task<List<CustomizedUINodeDTO>> GetAllAsync(int organizationId, string module, Cookie optionalLogin = null)
        {
            using var response = await SendGetRequestAsync(organizationId, module, optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<List<CustomizedUINodeDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetRequestAsync(int organizationId, string module, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie);
        }
    }
}
