using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceExhibitHelper
    {
        public static async Task<ItInterfaceExhibitDTO> CreateExhibit(int systemId, int interfaceId, Cookie optionalLogin = null)
        {
            using (var createdResponse = await SendCreateExhibitRequest(systemId,interfaceId,optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceExhibitDTO>();

                Assert.Equal(interfaceId,response.Id);
                Assert.Equal(systemId,response.ItSystemId);

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

            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"api/exhibit/{interfaceId}?organizationId=-1" /*org id not used*/), cookie);
        }

        public static async Task<IReadOnlyList<ItInterfaceExhibitUsageDTO>> GetExhibitInterfaceUsages(int contractId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/ItInterfaceExhibitUsage?contractId={contractId}");
            using (var result = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                var exhibitUsages =
                    await result.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItInterfaceExhibitUsageDTO>>();
                return exhibitUsages.ToList().AsReadOnly();
            }
        }

        public static async Task<ItInterfaceExhibitUsageDTO> CreateExhibitUsage(int contractId, int usageId, int exhibitId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                itContractId = contractId,
            };

            using (var createdResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/itInterfaceExhibitUsage?usageId={usageId}&exhibitId={exhibitId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceExhibitUsageDTO>();

                Assert.Equal(contractId, response.ItContractId);
                Assert.Equal(usageId, response.ItSystemUsageId);
                Assert.Equal(exhibitId, response.ItInterfaceExhibitId);

                return response;
            }
        }
    }
}
