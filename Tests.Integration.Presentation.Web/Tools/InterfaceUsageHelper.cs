using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceUsageHelper
    {
        public static async Task<HttpResponseMessage> GetItInterfaceUsageResponse(int usageId, int systemId, int interfaceId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var oldInterfaceUrl = TestEnvironment.CreateUrl($"api/ItInterfaceUsage?" +
                                                            $"usageId={usageId}&" +
                                                            $"sysId={systemId}&" +
                                                            $"interfaceId={interfaceId}");

            return await HttpApi.GetWithCookieAsync(oldInterfaceUrl, cookie);
        }
    }
}