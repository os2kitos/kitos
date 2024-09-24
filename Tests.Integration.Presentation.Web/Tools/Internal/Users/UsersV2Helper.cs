using Core.DomainModel.Organization;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.User;
using Xunit;
using Presentation.Web.Models.API.V2.Internal.Response.User;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Users
{
    public static class UsersV2Helper
    {
        public static async Task<UserResponseDTO> CreateUser(Guid organizationUuid, CreateUserRequestDTO request, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v2/internal/organization/{organizationUuid}/users/create"), requestCookie, request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendNotification(Guid organizationUuid, Guid userUuid,
            Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix()}/{userUuid}/organization/{organizationUuid}/notifications/send");
            return await HttpApi.PostWithCookieAsync(url, requestCookie, null);
        }

        private static string ControllerPrefix()
        {
            return "api/v2/internal/users";

        public static async Task<UserCollectionPermissionsResponseDTO> GetUserCollectionPermissions(Guid organizationUuid, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v2/internal/organization/{organizationUuid}/users/permissions"), requestCookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserCollectionPermissionsResponseDTO>();
        }

    }
}
