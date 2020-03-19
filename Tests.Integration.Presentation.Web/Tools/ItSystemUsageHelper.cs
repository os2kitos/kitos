using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Presentation.Web.Models.ItSystemUsage;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ItSystemUsageHelper
    {
        public static async Task<ItSystemUsageSensitiveDataLevelDTO> AddSensitiveDataLevel(int systemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/add"), cookie, sensitiveDataLevel))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> RemoveSensitiveDataLevel(int systemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/remove"), cookie, sensitiveDataLevel))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }
    }
}
