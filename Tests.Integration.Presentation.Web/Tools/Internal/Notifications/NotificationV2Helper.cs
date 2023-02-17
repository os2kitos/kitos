using System;
using System.Net;
using System.Net.Http;
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

        public static async Task<NotificationResponseDTO> CreateImmediateNotificationAsync(OwnerResourceType ownerResourceType, Guid organizationUuid, ImmediateNotificationWriteRequestDTO body)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/immediate?organizationUuid={organizationUuid}"), cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }
        public static async Task<NotificationResponseDTO> CreateScheduledNotificationAsync(OwnerResourceType ownerResourceType, Guid organizationUuid, ScheduledNotificationWriteRequestDTO body)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/scheduled?organizationUuid={organizationUuid}"), cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }
        public static async Task<NotificationResponseDTO> UpdateScheduledNotificationAsync(OwnerResourceType ownerResourceType, Guid notificationUuid, UpdateScheduledNotificationWriteRequestDTO body)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/scheduled/{notificationUuid}"), cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<NotificationResponseDTO> DeactivateNotificationAsync(OwnerResourceType ownerResourceType, Guid notificationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/scheduled/deactivate/{notificationUuid}"), cookie, new{});
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task DeleteNotificationAsync(OwnerResourceType ownerResourceType, Guid notificationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/{notificationUuid}"), cookie);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static async Task<NotificationResponseDTO> GetNotificationByUuid(OwnerResourceType ownerResourceType, Guid uuid)
        {
            using var response = await SendGetNotificationByUuid(ownerResourceType, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetNotificationByUuid(OwnerResourceType ownerResourceType, Guid uuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/{uuid}"), cookie);
        }
    }
}
