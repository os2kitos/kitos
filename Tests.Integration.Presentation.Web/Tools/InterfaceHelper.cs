﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceHelper
    {
        public static ItInterfaceDTO CreateInterfaceDto(
            string name,
            string interfaceId,
            int orgId,
            AccessModifier access,
            string notes = null)
        {
            return new ItInterfaceDTO
            {
                ItInterfaceId = interfaceId,
                Name = name,
                OrganizationId = orgId,
                AccessModifier = access,
                Note = notes
            };
        }

        public static async Task<HttpResponseMessage> SendDeleteInterfaceRequestAsync(int id)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/itinterface/{id}?{KitosApiConstants.UnusedOrganizationIdParameter}"); //Org id not used

            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }

        public static async Task<ItInterfaceDTO> CreateInterface(ItInterfaceDTO input)
        {
            using var createdResponse = await SendCreateInterface(input);
            Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
            return await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceDTO>();
        }

        public static async Task<HttpResponseMessage> SendCreateInterface(ItInterfaceDTO input, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("api/itinterface");

            return await HttpApi.PostWithCookieAsync(url, cookie, input);
        }

        public static async Task CreateInterfaces(params ItInterfaceDTO[] interfaces)
        {
            foreach (var dto in interfaces)
            {
                await CreateInterface(dto);
            }
        }

        public static async Task<DataRowDTO> CreateDataRowAsync(int organizationId, int interfaceId, Cookie optionalLogin = null)
        {
            using (var response = await SendCreateDataRowRequestAsync(organizationId, interfaceId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<DataRowDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendCreateDataRowRequestAsync(int organizationId, int interfaceId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"/api/dataRow?organizationId={organizationId}");

            var body = new
            {
                itInterfaceId = interfaceId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<HttpResponseMessage> SendGetInterfaceRequestAsync(int interfaceId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/itinterface/{interfaceId}");

            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        public static async Task<ItInterfaceDTO> SetUrlAsync(int interfaceId, string url, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var response = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/itinterface/{interfaceId}?{KitosApiConstants.UnusedOrganizationIdParameter}"), cookie, new { url = url }))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceDTO>();
            }
        }

        public static async Task<ItInterfaceDTO> GetInterfaceById(int interfaceId, Cookie optionalLogin = null)
        {
            using (var response = await SendGetInterfaceRequestAsync(interfaceId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceDTO>();
            }
        }


        public static async Task<HttpResponseMessage> SendChangeNameRequestAsync(int interfaceId, string newName, int orgId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/itInterface/{interfaceId}?organizationId={orgId}");
            var body = new
            {
                name = newName
            };

            using (var response = await HttpApi.PatchWithCookieAsync(url, cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return response;
            }
        }

        public static async Task<IEnumerable<ItInterfaceDTO>> GetInterfacesAsync(Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"odata/ItInterfaces");

            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadOdataListResponseBodyAsAsync<ItInterfaceDTO>();
            }

        }

        public static async Task DeleteInterfaceAsync(int itInterfaceId)
        {
            using var response = await SendDeleteInterfaceRequestAsync(itInterfaceId);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
