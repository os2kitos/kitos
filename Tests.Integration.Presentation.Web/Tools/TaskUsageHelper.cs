using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class TaskUsageHelper
    {
        public static async Task<TaskUsageDTO> CreateTaskUsageAsync(int organizationUnitId, int taskRefId, Cookie optionalCookie = null)
        {
            var cookie = optionalCookie ?? await HttpApi.GetCookieAsync(Core.DomainModel.Organization.OrganizationRole.GlobalAdmin);
            var body = new CreateTaskUsageDTO
            {
                TaskRefId = taskRefId,
                OrgUnitId = organizationUnitId
            };

            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/taskUsage"), cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<TaskUsageDTO>();
        }
    }
}
