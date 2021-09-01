using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceExhibitHelper
    {
        public static async Task<ItInterfaceExhibitDTO> CreateExhibit(int systemId, int interfaceId, Cookie optionalLogin = null)
        {
            using (var createdResponse = await SendCreateExhibitRequest(systemId, interfaceId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceExhibitDTO>();

                Assert.Equal(interfaceId, response.Id);
                Assert.Equal(systemId, response.ItSystemId);

                return response;
            }
        }

        public static async Task<HttpResponseMessage> SendCreateExhibitRequest(int systemId, int interfaceId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                itInterfaceId = interfaceId,
                itSystemId = systemId
            };

            return await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"api/exhibit"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendRemoveExhibitRequest(int interfaceId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"api/exhibit/{interfaceId}?{KitosApiConstants.UnusedOrganizationIdParameter}" /*org id not used*/), cookie);
        }
    }
}