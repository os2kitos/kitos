using System;
using Core.DomainModel.Organization;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Users;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class UserHelper
    {
        public static async Task<List<UserWithOrganizationResponseDTO>> GetUsersWithRightsholderAccessAsync(Cookie optionalLogin = null)
        {
            using var response = await SendGetUsersWithRightsholderAccessAsync(optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<List<UserWithOrganizationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetUsersWithRightsholderAccessAsync(Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/v2/internal/users/with-rightsholder-access"), cookie);
        }

        public static async Task<List<UserWithCrossOrganizationalRightsResponseDTO>> GetUsersWithCrossAccessAsync(Cookie optionalLogin = null)
        {
            using var response = await SendGetUsersWithCrossAccessAsync(optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<List<UserWithCrossOrganizationalRightsResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetUsersWithCrossAccessAsync(Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/v2/internal/users/with-cross-organization-permissions"), cookie);
        }

        public static async Task<HttpResponseMessage> SendDeleteUserAsync(int userId, Cookie optionalLogin = null, int organizationId = 0)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var request = $"api/v1/user/delete/{userId}";
            if (organizationId != 0)
            {
                return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl(request + $"/{organizationId}"), cookie);
            }
            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl(request), cookie);
        }

        public static async Task<List<UserWithEmailDTO>> SearchUsersAsync(string query, Cookie optionalLogin = null)
        {
            using var response = await SendSearchUsersAsync(query, optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<List<UserWithEmailDTO>>();
        }

        public static async Task<HttpResponseMessage> SendSearchUsersAsync(string query, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/users/search?query={query}"), cookie);
        }

        public static async Task<UserAdministrationPermissionsDTO> GetUserAdministrationPermissionsAsync(Guid organizationUuid,Cookie optionalLogin = null)
        {
            using var response = await SendGetUserAdministrationPermissionsRequestAsync(organizationUuid,optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<UserAdministrationPermissionsDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetUserAdministrationPermissionsRequestAsync(Guid organizationUuid,Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid:D}/administration/users/permissions");
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }
    }
}
