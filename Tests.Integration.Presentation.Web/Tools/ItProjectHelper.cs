using System;
using System.Net;
using System.Net.Http;
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

            var body = new
            {
                name = name,
                organizationId = organizationId,
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/itproject"), cookie, body))
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

        public static async Task<GoalDTO> AddGoalAsync(int projectId, string humanReadableId, bool measurable, string name, string description, DateTime goalDate1, DateTime goalDate2, DateTime goalDate3, Cookie optionalLogin = null)
        {
            using (var response = await SendAddGoalAsyncRequestAsync(projectId, humanReadableId, measurable, name, description, goalDate1, goalDate2, goalDate3, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<GoalDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddGoalAsyncRequestAsync(int projectId, string humanReadableId, bool measurable, string name, string description, DateTime goalDate1, DateTime goalDate2, DateTime goalDate3, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl("api/goal/");
            var body = new
            {
                description = description,
                goalStatusId = projectId, //yep that's not a mistake. goalStatusId ::= project.id
                goalTypeId = "2",
                measurable = measurable,
                name = name,
                status = 1,
                subGoalDate1 = goalDate1,
                subGoalDate2 = goalDate2,
                subGoalDate3 = goalDate3,
                humanReadableId = humanReadableId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }
    }
}
