using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1.UI_Configuration;
using Tests.Integration.Presentation.Web.UI_Configuration;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.UI_Configuration
{
    public class UIConfigurationHelper
    {
        private static readonly Random _random = new Random(DateTime.Now.Millisecond);

        public static async Task<HttpResponseMessage> SendPutRequestAsync(int organizationId, string module, UIModuleCustomizationDTO body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie, body);
        }

        public static async Task<UIModuleCustomizationDTO> GetCustomizationByModuleAsync(int organizationId, string module)
        {
            using var response = await SendGetRequestAsync(organizationId, module);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<UIModuleCustomizationDTO>();
        }

        public static async Task<UIModuleCustomizationDTO> CreateUIModuleAndSaveAsync(int organizationId, string module,
            Cookie optionalLogin = null)
        {
            var dto = PrepareTestUiModuleCustomizationDto();
            using var response = await SendPutRequestAsync(organizationId, module, dto, optionalLogin);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return dto;
        }

        public static async Task<HttpResponseMessage> SendGetRequestAsync(int organizationId, string module, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie);
        }
        
        public static UIModuleCustomizationDTO PrepareTestUiModuleCustomizationDto(int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            return new UIModuleCustomizationDTO
            {
                Nodes = PrepareTestNodes(numberOfElements, key, isEnabled)
            };
        }
        
        public static List<CustomizedUINodeDTO> PrepareTestNodes(int numberOfElements = 1, string key = "", bool isEnabled = false, params string[] keys)
        {
            var nodes = new List<CustomizedUINodeDTO>();
            for (var i = 1; i <= numberOfElements; i++)
            {
                key = string.IsNullOrEmpty(key) ? RandomString(i) : key;
                nodes.Add(new CustomizedUINodeDTO { Key = key, Enabled = isEnabled });
                key = "";
            }

            return nodes;
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static string CreateName()
        {
            return $"{nameof(UIConfigurationTests)}{Guid.NewGuid()}";
        }

        public static string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }
    }
}
