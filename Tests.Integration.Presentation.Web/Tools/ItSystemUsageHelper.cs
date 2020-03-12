using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.ItSystemUsage;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ItSystemUsageHelper
    {
        public static async Task<ItSystemUsageSensitiveDataLevelDTO> AddSensitiveDataLevel(int systemUsageId, int sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/add/{sensitiveDataLevel}"), cookie, null))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> RemoveSensitiveDataLevel(int systemUsageId, int sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/remove/{sensitiveDataLevel}"), cookie, null))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }
    }
}
