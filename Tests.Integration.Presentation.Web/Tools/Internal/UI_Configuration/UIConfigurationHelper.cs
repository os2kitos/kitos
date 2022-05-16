using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.GDPR;
using Presentation.Web.Models.API.V1.UI_Configuration;
using Tests.Integration.Presentation.Web.UI_Configuration;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.UI_Configuration
{
    public class UIConfigurationHelper
    {
        private static Random _random = new Random();

        public static async Task<HttpResponseMessage> SendPutRequestAsync(int organizationId, string module, UIModuleCustomizationDTO body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            return await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/organizations/{organizationId}/ui-config/modules/{module}"), cookie, body);
        }

        public static async Task<List<UIModuleCustomizationDTO>> GetAllAsync(int organizationId, string module)
        {
            using var response = await SendGetRequestAsync(organizationId, module);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<List<UIModuleCustomizationDTO>>();
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
            for (var i = 0; i < numberOfElements; i++)
            {
                key = string.IsNullOrEmpty(key) ? RandomString(10) : key;
                nodes.Add(new CustomizedUINodeDTO { Key = key, Enabled = isEnabled });
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
            return $"{nameof(UIConfigurationTests)}{RandomString(10)}";
        }

        public static string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }
    }
}
