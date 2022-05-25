using Core.DomainModel.Organization;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Users;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class UserHelper
    {
        public static async Task<List<UserWithOrganizationDTO>> GetUsersWithRightsholderAccessAsync(Cookie optionalLogin = null)
        {
            using var response = await SendGetUsersWithRightsholderAccessAsync(optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<List<UserWithOrganizationDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetUsersWithRightsholderAccessAsync(Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/user/with-rightsholder-access"), cookie);
        }

        public static async Task<List<UserWithCrossOrganizationalRightsDTO>> GetUsersWithCrossAccessAsync(Cookie optionalLogin = null)
        {
            using var response = await SendGetUsersWithCrossAccessAsync(optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<List<UserWithCrossOrganizationalRightsDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetUsersWithCrossAccessAsync(Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/user/with-cross-organization-permissions"), cookie);
        }

        public static User GetUserByIdWithRightsAsync(int id)
        {
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable()
                .Include(user => user.DataProcessingRegistrationRights)
                .Include(user => user.OrganizationRights)
                .Include(user => user.ItContractRights)
                .Include(user => user.ItProjectRights)
                .Include(user => user.ItSystemRights)
                .Include(user => user.OrganizationUnitRights)
                .Include(user => user.SsoIdentities)
                .Include(user => user.ItProjectStatuses)
                .Include(user => user.ResponsibleForCommunications)
                .Include(user => user.HandoverParticipants)
                .Include(user => user.ResponsibleForRisks)
                .ById(id));
            
            return user;
        }

        public static async Task<HttpResponseMessage> SendDeleteUserAsync(int userId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"api/user/{userId}"), cookie);
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
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/users/search/{query}"), cookie);
        }
    }
}
