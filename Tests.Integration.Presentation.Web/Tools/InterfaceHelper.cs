using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceHelper
    {
        public static ItInterfaceDTO CreateInterfaceDto(
            string name,
            string interfaceId,
            int? userId,
            int orgId,
            AccessModifier access)
        {
            return new ItInterfaceDTO
            {
                ItInterfaceId = interfaceId,
                Name = name,
                OrganizationId = orgId,
                BelongsToId = userId,
                AccessModifier = access
            };
        }

        public static async Task<HttpResponseMessage> SendDeleteInterfaceRequestAsync(int id)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/itinterface/{id}?organizationId=-1"); //Org id not used

            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }

        public static async Task<ItInterfaceDTO> CreateInterface(ItInterfaceDTO input)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("api/itinterface");

            using (var createdResponse = await HttpApi.PostWithCookieAsync(url, cookie, input))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                return await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceDTO>();
            }
        }

        public static async Task CreateInterfaces(params ItInterfaceDTO[] interfaces)
        {
            foreach (var dto in interfaces)
            {
                await CreateInterface(dto);
            }
        }

        public static async Task<DataRowDTO> CreateDataRowAsync(int interfaceId, Cookie optionalLogin = null)
        {
            using (var response = await SendCreateDataRowRequestAsync(interfaceId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<DataRowDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendCreateDataRowRequestAsync(int interfaceId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("/api/dataRow/");

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

        public static async Task<ItInterfaceDTO> GetInterfaceById(int interfaceId, Cookie optionalLogin = null)
        {

            using (var response = await SendGetInterfaceRequestAsync(interfaceId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceDTO>();
            }

        }
    }
}
