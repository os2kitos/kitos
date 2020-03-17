using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.Qa;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class BrokenExternalReferencesReportHelper
    {
        public static async Task<BrokenExternalReferencesReportStatusDTO> GetStatusAsync()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl("api/v1/broken-external-references-report/status");
            using (var result = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                return await result.ReadResponseBodyAsKitosApiResponseAsync<BrokenExternalReferencesReportStatusDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendGetCurrentCsvAsync()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl("api/v1/broken-external-references-report/current/csv");
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        public static async Task TriggerRequestAsync()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl("api/v1/broken-external-references-report/trigger");
            using (var response = await HttpApi.PostWithCookieAsync(url, cookie, new { }))
            {
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            }
        }
    }
}
