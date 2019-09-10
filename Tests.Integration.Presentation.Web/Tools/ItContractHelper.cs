using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class ItContractHelper
    {
        public static async Task<ItContractDTO> CreateContract(string name, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                name = name,
                organizationId = organizationId,
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/itcontract"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItContractDTO>();

                Assert.Equal(organizationId, response.OrganizationId);
                Assert.Equal(name, response.Name);
                return response;
            }
        }

        public static async Task<ItContractDTO> GetItContract(int contractId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/ItContract/{contractId}");
            using (var result = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                var contract = await result.ReadResponseBodyAsKitosApiResponseAsync<ItContractDTO>();
                return contract;
            }
        }

        public static async Task<ItSystemUsageSimpleDTO> AddItSystemUsage(int contractId, int usageId, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                systemUsageId = usageId,
                organizationId = organizationId,
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"api/itcontract/{contractId}?systemUsageId={usageId}&organizationId={organizationId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemUsageSimpleDTO>>();

                var addedMapping = response.FirstOrDefault(x=>x.Id == usageId);
                Assert.NotNull(addedMapping);
                return addedMapping;
            }
        }
    }
}
