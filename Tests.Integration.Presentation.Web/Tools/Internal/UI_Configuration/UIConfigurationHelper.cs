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

        public static async Task<HttpResponseMessage> SendPutRequestAsync(int organizationId, string module, UIModuleCustomizationDTO body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie, body);
        }

        public static async Task<UIModuleCustomizationDTO> GetAllAsync(int organizationId, string module, Cookie optionalLogin = null)
        {
            using var response = await SendGetRequestAsync(organizationId, module, optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<UIModuleCustomizationDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetRequestAsync(int organizationId, string module, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie);
        }
    }
}
