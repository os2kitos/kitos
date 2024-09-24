using Core.DomainModel.Organization;
using System;
using System.Net;
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
                    $"api/v2/internal/users/organization/{organizationUuid}/create"), requestCookie, request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<UserResponseDTO>();
        }

    }
}
