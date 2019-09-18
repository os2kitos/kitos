using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class LocalConfigHelper
    {

        public static async Task<HttpResponseMessage> SendUpdateConfigRequestAsync(object body, int orgId, Cookie optionalCookie = null)
        {
            var cookie = optionalCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl($"/odata/Configs({orgId})?organizationId={orgId}");

            return await HttpApi.PatchWithCookieAsync(url, cookie, body);
        }
    }
}
