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
            using (var response = await SendSetUsageDataWorkerRequestAsync(systemUsageId,organizationId,optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDataWorkerRelationDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendSetUsageDataWorkerRequestAsync(int systemUsageId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("/api/UsageDataworker/");

            var body = new
            {
                ItSystemUsageId = systemUsageId,
                DataWorkerId = organizationId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<ItSystemDataWorkerRelationDTO> SetDataWorkerAsync(int systemId, int organizationId, Cookie optionalLogin = null)
        {
            using (var response = await SendSetDataWorkerRequestAsync(systemId, organizationId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItSystemDataWorkerRelationDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendSetDataWorkerRequestAsync(int systemId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("/api/Dataworker/");

            var body = new
            {
                ItSystemId = systemId,
                DataWorkerId = organizationId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<WishDTO> CreateWishAsync(int systemUsageId, string text, Cookie optionalLogin = null, int? userId = null)
        {
            using (var response = await SendCreateWishRequestAsync(systemUsageId, text, optionalLogin, userId))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<WishDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendCreateWishRequestAsync(int systemUsageId, string text, Cookie optionalLogin = null, int? userId = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl("api/wish/");
            var body = new
            {
                userId = userId ?? TestEnvironment.DefaultUserId,
                text = text,
                itSystemUsageId = systemUsageId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }
    }
}
