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
        private const string _basePath = "api/v2/notifications";

        public static async Task<NotificationResponseDTO> CreateImmediateNotificationAsync(OwnerResourceType ownerResourceType, ImmediateNotificationWriteRequestDTO body, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/immediate"), cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }
        public static async Task<NotificationResponseDTO> CreateScheduledNotificationAsync(OwnerResourceType ownerResourceType, ScheduledNotificationWriteRequestDTO body, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/scheduled"), cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }
        public static async Task<NotificationResponseDTO> UpdateScheduledNotificationAsync(OwnerResourceType ownerResourceType, Guid notificationUuid, UpdateScheduledNotificationWriteRequestDTO body, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/scheduled/{notificationUuid}"), cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<NotificationResponseDTO> DeactivateNotificationAsync(OwnerResourceType ownerResourceType, Guid notificationUuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/scheduled/deactivate/{notificationUuid}"), cookie, new{});
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task DeleteNotificationAsync(OwnerResourceType ownerResourceType, Guid notificationUuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/{notificationUuid}"), cookie);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static async Task<IEnumerable<NotificationResponseDTO>> GetNotificationsAsync(
            OwnerResourceType ownerResourceType,
            Guid organizationUuid,
            DateTime? fromDate = null,
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

            if (fromDate.HasValue)
                criteria.Add(new KeyValuePair<string, string>("fromDate", fromDate.Value.ToString("O")));

            var joinedCriteria = string.Join("&", criteria.Select(x => $"{x.Key}={x.Value}"));
            var queryString = joinedCriteria.Any() ? $"?{joinedCriteria}" : string.Empty;

            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/{queryString}"), cookie);

            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            return await response.ReadResponseBodyAsAsync<IEnumerable<NotificationResponseDTO>>();
        }

        public static async Task<NotificationResponseDTO> GetNotificationByUuid(OwnerResourceType ownerResourceType, Guid uuid, Cookie userCookie = null)
        {
            using var response = await SendGetNotificationByUuid(ownerResourceType, uuid, userCookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<NotificationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetNotificationByUuid(OwnerResourceType ownerResourceType, Guid uuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{_basePath}/{ownerResourceType}/{uuid}"), cookie);
        }
    }
}
