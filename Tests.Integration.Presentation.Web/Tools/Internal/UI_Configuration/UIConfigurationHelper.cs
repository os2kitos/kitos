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

        public static async Task<HttpResponseMessage> SendPutRequestAsync(int organizationId, string module, UIModuleCustomizationDTO body, Cookie optionalLogin)
        {
            return await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), optionalLogin, body);
        }

        public static async Task<HttpResponseMessage> GetCustomizationByModuleAsync(int organizationId, string module)
        {
            return await SendGetRequestAsync(organizationId, module);
        }

        public static async Task<UIModuleCustomizationDTO> CreateUIModuleAndSaveAsync(int organizationId, string module,
            Cookie optionalLogin)
        {
            var dto = PrepareTestUiModuleCustomizationDto();
            using var response = await SendPutRequestAsync(organizationId, module, dto, optionalLogin);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return dto;
        }

        public static async Task<HttpResponseMessage> SendGetRequestAsync(int organizationId, string module, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie);
        }
        
        public static UIModuleCustomizationDTO PrepareTestUiModuleCustomizationDto(int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            return new UIModuleCustomizationDTO
            {
                Nodes = PrepareTestNodes(numberOfElements, key, isEnabled)
            };
        }
        
        public static List<CustomizedUINodeDTO> PrepareTestNodes(int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            if (numberOfElements < 1)
                throw new ArgumentNullException("NumberOfElements cannot be lower than 1");

            var nodes = new List<CustomizedUINodeDTO>();
            for (var i = 1; i <= numberOfElements; i++)
            {
                //if the "key" parameter is empty create a random new key, otherwise use the parameter
                //node key can only contain letters and dots
                var nodeKey = string.IsNullOrEmpty(key) ? RandomString(numberOfElements) : key;
                nodes.Add(new CustomizedUINodeDTO { Key = nodeKey, Enabled = isEnabled });
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
