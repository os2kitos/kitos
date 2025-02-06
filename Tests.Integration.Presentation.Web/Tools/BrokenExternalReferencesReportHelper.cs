using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.QA;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class BrokenExternalReferencesReportHelper
    {
        private const string _basePath = "api/v2/internal/broken-external-references-report";

        public static async Task<BrokenExternalReferencesReportStatusResponseDTO> GetStatusAsync()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl(_basePath + "/status");
            using var result = await HttpApi.GetWithCookieAsync(url, cookie);
            return await result.ReadResponseBodyAsAsync<BrokenExternalReferencesReportStatusResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetCurrentCsvAsync()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl(_basePath + "/current/csv");
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        public static async Task TriggerRequestAsync()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl(_basePath + "/trigger");
            using var response = await HttpApi.PostWithCookieAsync(url, cookie, new { });
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }
    }
}
