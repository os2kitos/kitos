using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ItSystemHelper
    {
        public static async Task<ItSystemDTO> CreateItSystemInOrganizationAsync(string itSystemName, int orgId, AccessModifier accessModifier)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var itSystem = new
            {
                name = itSystemName,
                belongsToId = orgId,
                organizationId = orgId,
                AccessModifier = accessModifier
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/itsystem"), cookie, itSystem))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemDTO>();

                Assert.Equal(orgId, response.OrganizationId);
                Assert.Equal(orgId, response.BelongsToId);
                Assert.Equal(itSystemName, response.Name);
                return response;
            }
        }

        public static async Task<ItSystemUsageDTO> TakeIntoUseAsync(int itSystemId, int orgId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var itSystem = new
            {
                itSystemId = itSystemId,
                organizationId = orgId,
                dataLevel = "NONE",
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/itSystemUsage"), cookie, itSystem))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDTO>();

                Assert.Equal(orgId, response.OrganizationId);
                Assert.Equal(itSystemId, response.ItSystemId);
                return response;
            }
        }

        public static async Task<ItSystemUsageDTO> GetItSystemUsage(int itSystemUsageId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/ItSystemUsage/{itSystemUsageId}");
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDTO>();
            }
        }

        public static async Task<ItSystemUsageDataWorkerRelationDTO> SetUsageDataWorkerAsync(int systemUsageId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("/api/UsageDataworker/");

            var body = new
            {
                ItSystemUsageId = systemUsageId,
                DataWorkerId = organizationId
            };

            using (var response = await HttpApi.PostWithCookieAsync(url, cookie, body))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDataWorkerRelationDTO>();
            }
        }
    }
}
