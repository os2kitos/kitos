using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private const string BasePath = "api/v2/internal/notifications";

        public static async Task<NotificationResponseDTO> CreateImmediateNotificationAsync(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, ImmediateNotificationWriteRequestDTO body, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{ownerResourceUuid}/immediate"), cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }
        public static async Task<NotificationResponseDTO> CreateScheduledNotificationAsync(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, ScheduledNotificationWriteRequestDTO body, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{ownerResourceUuid}/scheduled"), cookie, body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<NotificationResponseDTO> UpdateScheduledNotificationAsync(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, Guid notificationUuid, UpdateScheduledNotificationWriteRequestDTO body, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{ownerResourceUuid}/scheduled/{notificationUuid}"), cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<NotificationResponseDTO> DeactivateNotificationAsync(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, Guid notificationUuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{ownerResourceUuid}/scheduled/deactivate/{notificationUuid}"), cookie, new{});

            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task DeleteNotificationAsync(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, Guid notificationUuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{ownerResourceUuid}/{notificationUuid}"), cookie);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static async Task<IEnumerable<NotificationResponseDTO>> GetNotificationsAsync(
            OwnerResourceType ownerResourceType,
            Guid organizationUuid,
            Guid? ownerResourceUuid = null,
            bool onlyActive = false,
            int? page = null,
            int? pageSize = null,
            Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var criteria = new List<KeyValuePair<string, string>>
            {
                new ("organizationUuid", organizationUuid.ToString("D"))
            };

            if (page.HasValue)
                criteria.Add(new KeyValuePair<string, string>("page", page.Value.ToString("D")));

            if (pageSize.HasValue)
                criteria.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (ownerResourceUuid.HasValue)
                criteria.Add(new KeyValuePair<string, string>("ownerResourceUuid", ownerResourceUuid.Value.ToString("D")));

            if (onlyActive)
                criteria.Add(new KeyValuePair<string, string>("onlyActive", true.ToString()));

            var joinedCriteria = string.Join("&", criteria.Select(x => $"{x.Key}={x.Value}"));
            var queryString = joinedCriteria.Any() ? $"?{joinedCriteria}" : string.Empty;

            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{queryString}"), cookie);

            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            return await response.ReadResponseBodyAsAsync<IEnumerable<NotificationResponseDTO>>();
        }

        public static async Task<NotificationResponseDTO> GetNotificationByUuid(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, Guid uuid, Cookie userCookie = null)
        {
            using var response = await SendGetNotificationByUuid(ownerResourceType, ownerResourceUuid, uuid, userCookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetNotificationByUuid(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, Guid uuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{ownerResourceUuid}/{uuid}"), cookie);
        }

        public static async Task<NotificationResourcePermissionsDTO> GetPermissionsAsync(OwnerResourceType ownerResourceType, Guid ownerResourceUuid, Guid notificationUuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{BasePath}/{ownerResourceType}/{ownerResourceUuid}/{notificationUuid}/permissions"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResourcePermissionsDTO>();
        }
    }
}
