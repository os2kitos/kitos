using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class ItProjectHelper
    {
        public static async Task<ItProjectDTO> CreateProject(string name, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var itSystem = new
            {
                name = name,
                organizationId = organizationId,
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/itproject"), cookie, itSystem))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItProjectDTO>();

                Assert.Equal(organizationId, response.OrganizationId);
                Assert.Equal(name, response.Name);
                return response;
            }
        }

        public static async Task<ItSystemUsageDTO> AddSystemBinding(int projectId, int usageId, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"api/itproject/{projectId}?usageId={usageId}&organizationId={organizationId}"), cookie, string.Empty))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDTO>();

                Assert.Equal(usageId, response.Id);
                return response;
            }
        }
    }
}
