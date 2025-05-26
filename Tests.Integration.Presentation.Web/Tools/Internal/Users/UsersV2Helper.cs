using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Internal.Request.User;
using Presentation.Web.Models.API.V2.Request.User;
using Xunit;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using System.Linq;
using System.Linq.Expressions;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Users
{
    public static class UsersV2Helper
    {
        public static async Task<UserResponseDTO> CreateUser(Guid organizationUuid, CreateUserRequestDTO request, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"{ControllerPrefix(organizationUuid)}/create"), requestCookie, request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserResponseDTO>();
        }

        public static async Task<UserResponseDTO> UpdateUser(Guid organizationUuid, Guid userUuid,
            object request, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"{ControllerPrefix(organizationUuid)}/{userUuid}/patch"), requestCookie, request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendNotification(Guid organizationUuid, Guid userUuid,
            Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix(organizationUuid)}/{userUuid}/notifications/send");
            return await HttpApi.PostWithCookieAsync(url, requestCookie, null);
        }

        public static async Task<HttpStatusCode> DeleteUserAndVerifyStatusCode(Guid organizationUuid, Guid userUuid, Cookie cookie = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl(
                $"{ControllerPrefix(organizationUuid)}/{userUuid}");
            using var response = await HttpApi.DeleteWithCookieAsync(url,
                requestCookie);

            var statusCode = response.StatusCode;
            Assert.Equal(expectedStatusCode, statusCode);
            return statusCode;
        }

        public static async Task DeleteUserGlobally(Guid userUuid, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl(
                $"api/v2/internal/users/{userUuid}");
            using var response = await HttpApi.DeleteWithCookieAsync(url,
                requestCookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task<UserCollectionPermissionsResponseDTO> GetUserCollectionPermissions(Guid organizationUuid, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"{ControllerPrefix(organizationUuid)}/permissions"), requestCookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserCollectionPermissionsResponseDTO>();
        }

        public static async Task<UserIsPartOfCurrentOrgResponseDTO> GetUserByEmail(Guid organizationUuid, string email, Cookie cookie = null)
        {
            using var response = await SendGetUserByEmail(organizationUuid, email, cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserIsPartOfCurrentOrgResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetUserByEmail(Guid organizationUuid, string email, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"{ControllerPrefix(organizationUuid)}/find-any-by-email?email={email}"), requestCookie);
        }

        public static async Task<IEnumerable<UserReferenceResponseDTO>> GetUsers(string email)
        {
            var requestCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new ("emailQuery", email)
            };

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            using var response = await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"{GlobalUserControllerPrefix()}/search?{query}"), requestCookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<UserReferenceResponseDTO>>();
        }

        public static async Task<IEnumerable<OrganizationResponseDTO>> GetUserOrganization(Guid userUuid)
        {
            var requestCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var response = await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"{GlobalUserControllerPrefix()}/{userUuid}/organizations"), requestCookie);

            var resp = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<OrganizationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> CopyRoles(Guid organizationUuid, Guid fromUser, Guid toUser,
            MutateUserRightsRequestDTO request)
        {
            var requestCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix(organizationUuid)}/{fromUser}/copy-roles/{toUser}");
            return await HttpApi.PostWithCookieAsync(url, requestCookie, request);
        }

        public static async Task<HttpResponseMessage> TransferRoles(Guid organizationUuid, Guid fromUser, Guid toUser,
            MutateUserRightsRequestDTO request)
        {
            var requestCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix(organizationUuid)}/{fromUser}/transfer-roles/{toUser}");
            return await HttpApi.PostWithCookieAsync(url, requestCookie, request);
        }

        public static async Task<IEnumerable<UserReferenceResponseDTO>> GetGlobalAdmins()
        {
            var requestCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/global-admins");
            using var response = await HttpApi.GetWithCookieAsync(url, requestCookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<UserReferenceResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> AddGlobalAdmin(Guid userUuid, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/global-admins/{userUuid}");
            return await HttpApi.PostWithCookieAsync(url, cookie, null);
        }

        public static async Task<HttpResponseMessage> RemoveGlobalAdmin(Guid userUuid, Cookie cookie = null)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/global-admins/{userUuid}");
            return await HttpApi.DeleteWithCookieAsync(url, requestCookie, null);
        }

        public static async Task<IEnumerable<UserReferenceWithOrganizationResponseDTO>> GetLocalAdmins()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/local-admins");
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<UserReferenceWithOrganizationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> AddLocalAdmin(Guid organizationUuid, Guid userUuid, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/{organizationUuid}/local-admins/{userUuid}");
            return await HttpApi.PostWithCookieAsync(url, cookie, null);
        }

        public static async Task<HttpResponseMessage> RemoveLocalAdmin(Guid organizationUuid, Guid userUuid, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/{organizationUuid}/local-admins/{userUuid}");
            return await HttpApi.DeleteWithCookieAsync(url, cookie, null);
        }


        public static async Task<UserResponseDTO> GetUser(Guid organizationUuid, Guid userUuid, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var cookie = await HttpApi.GetCookieAsync(role);

            var url = TestEnvironment.CreateUrl($"{ControllerPrefix(organizationUuid)}/{userUuid}");
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserResponseDTO>();
        }

        public static async Task<IEnumerable<UserReferenceResponseDTO>> GetSystemIntegrators()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/system-integrators");
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);
            Assert.True(response.IsSuccessStatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<UserReferenceResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> UpdateSystemIntegrator(Guid userUuid,
            bool requestedSystemIntegratorStatus, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"{GlobalUserControllerPrefix()}/system-integrators/{userUuid}?requestedValue={requestedSystemIntegratorStatus}");
            return await HttpApi.PatchWithCookieAsync(url, cookie, null);
        }

        public static async Task SetDefaultUnit(Guid organizationUuid, Guid userUuid, Guid unitUuid, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix(organizationUuid)}/{userUuid}/default-unit/{unitUuid}");
            using var response = await HttpApi.PatchWithCookieAsync(url, cookie, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static async Task<IdentityNamePairResponseDTO> GetDefaultUnit(Guid organizationUuid, Guid userUuid, OrganizationRole role = OrganizationRole.GlobalAdmin)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix(organizationUuid)}/{userUuid}/default-unit");
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IdentityNamePairResponseDTO>();
        }

        public static async Task<HttpResponseMessage> PatchUserAsync(Guid organizationUuid, Guid userUuid, Cookie cookie = null, params KeyValuePair<string, object>[] kvpPairs)
        {
            var requestCookie = cookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix(organizationUuid)}/{userUuid}/patch");
            return await HttpApi.PatchWithCookieAsync(url, requestCookie, kvpPairs.ToDictionary(x => x.Key, x => x.Value));
        }

        public static async Task<HttpResponseMessage> PatchUserAsync<T>(
            Guid organizationUuid,
            Guid userUuid,
            Expression<Func<UpdateUserRequestDTO, T>> propertySelector,
            T value, Cookie cookie = null)
        {
            if (!(propertySelector.Body is MemberExpression m))
                throw new ArgumentException("Selector must be a simple member access", nameof(propertySelector));

            var propertyName = m.Member.Name;
            var kvp = new KeyValuePair<string, object>(propertyName, value);
            return await PatchUserAsync(organizationUuid, userUuid, cookie, kvp);
        }

        private static string ControllerPrefix(Guid organizationUuid)
        {
            return $"api/v2/internal/organization/{organizationUuid}/users";
        }

        private static string GlobalUserControllerPrefix()
        {
            return $"api/v2/internal/users";
        }
    }
}
