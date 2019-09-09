using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
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
    }
}
