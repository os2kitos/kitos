using System;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Notifications
{
    public class AlertsV2Helper
    {
        private const string ControllerPrefix = "api/v2/internal/alerts";

        public static async Task<HttpResponseMessage> DeleteAlertAsync(Guid alertUuid)
        {
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix}/{alertUuid}");
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }

        public static async Task<HttpResponseMessage> GetAlertsAsync(Guid organizationUuid, Guid userUuid,
            RelatedEntityType entityType)
        {
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix}/organization/{organizationUuid}/user/{userUuid}/{entityType}");
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }
    }
}
