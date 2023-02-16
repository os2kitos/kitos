using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Notifications
{
    public class NotificationV2Helper
    {
        private const string _basePath = "api/v2/notifications";

        public static async Task<NotificationResponseDTO> CreateNotificationAsync(OwnerResourceType ownerResourceType, Guid organizationUuid, ImmediateNotificationWriteRequestDTO body)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/immediate?organizationUuid={organizationUuid}"), cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<NotificationResponseDTO> GetNotificationByUuid(OwnerResourceType ownerResourceType, Guid uuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/{uuid}"), cookie);
            var res = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }
    }
}
