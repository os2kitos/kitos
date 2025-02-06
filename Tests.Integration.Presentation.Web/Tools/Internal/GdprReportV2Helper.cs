using Core.DomainModel.Organization;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tests.Integration.Presentation.Web.Tools.Internal
{
    public class GdprReportV2Helper
    {
        public static async Task<HttpResponseMessage> GetGdprReportAsync(Guid organizationUuid)
        {
            var url = TestEnvironment.CreateUrl($"api/v2/internal/gdpr-report/{organizationUuid}");
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }
    }
}
