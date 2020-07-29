using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools.Model;
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

        public static async Task<HttpResponseMessage> SendRemoveUsageAsync(int systemUsageId, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"api/itSystemUsage/{systemUsageId}?organizationId={organizationId}"), cookie);
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
            using (var response = await SendSetUsageDataWorkerRequestAsync(systemUsageId, organizationId, optionalLogin))
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

        public static async Task<AccessType> CreateAccessTypeAsync(int id, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("odata/AccessTypes");
            var body = new
            {
                ItSystemId = id,
                Name = name
            };

            using (var response = await HttpApi.PostWithCookieAsync(url, cookie, body))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsAsync<AccessType>();
            }
        }

        public static async Task EnableAccessTypeAsync(int systemUsageId, int accessTypeId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"odata/ItSystemUsages({systemUsageId})/AccessTypes/{accessTypeId}");

            using (var response = await HttpApi.PostWithCookieAsync(url, cookie, new{ }))
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        public static async Task<AccessType[]> GetEnabledAccessTypesAsync(int systemUsageId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"odata/ItSystemUsages({systemUsageId})?$select=Id&$expand=AccessTypes");

            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return (await response.ReadResponseBodyAsAsync<GetAccessTypesResponse>()).AccessTypes;
            }
        }

        public static async Task<HttpResponseMessage> DeleteItSystemAsync(int systemId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/itsystem/{systemId}?organizationId={organizationId}");

            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }

        public static async Task<HttpResponseMessage> SendSetParentSystemRequestAsync(
            int systemId,
            int parentSystemId,
            int organizationId,
            Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/itsystem/{systemId}?organizationId={organizationId}");
            var body = new
            {
                parentId = parentSystemId
            };

            return await HttpApi.PatchWithCookieAsync(url, cookie, body);
        }

        public static async Task<HttpResponseMessage> SendSetTaskRefOnSystemRequestAsync(
            int systemId,
            int taskRefId,
            int organizationId,
            Cookie login)
        {
            var cookie = login;

            var url = TestEnvironment.CreateUrl($"api/itsystem/{systemId}?taskId={taskRefId}&organizationId={organizationId}");
            var body = new
            {

            };
            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<HttpResponseMessage> SendGetSystemRequestAsync(int systemId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/itsystem/{systemId}");

            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        public static async Task<ItSystemDTO> GetSystemAsync(int systemId, Cookie optionalLogin = null)
        {
            using (var response = await SendGetSystemRequestAsync(systemId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItSystemDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendChangeSystemUsageArchiveDutyRequestAsync(int usageId, ArchiveDutyTypes duty)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/itsystemusage/{usageId}?{KitosApiConstants.UnusedOrganizationIdParameter}");
            var body = new
            {
                archiveDuty = (int)duty
            };

            return await HttpApi.PatchWithCookieAsync(url, cookie, body);
        }

        public static async Task<HttpResponseMessage> SendChangeArchiveDutyRecommendationRequestAsync(int systemId, ArchiveDutyRecommendationTypes recommendation)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/itsystem/{systemId}?{KitosApiConstants.UnusedOrganizationIdParameter}");
            var body = new
            {
                archiveDuty = (int)recommendation
            };

            return await HttpApi.PatchWithCookieAsync(url, cookie, body);
        }

        public static async Task<HttpResponseMessage> SendChangeArchiveDutyRecommendationCommentRequestAsync(int systemId, string comment)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/itsystem/{systemId}?{KitosApiConstants.UnusedOrganizationIdParameter}");
            var body = new
            {
                archiveDutyComment = comment
            };

            return await HttpApi.PatchWithCookieAsync(url, cookie, body);
        }
    }
}
