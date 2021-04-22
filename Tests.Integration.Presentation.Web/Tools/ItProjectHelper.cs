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

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"api/itproject?organizationId={organizationId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItProjectDTO>();

                Assert.Equal(organizationId, response.OrganizationId);
                Assert.Equal(name, response.Name);
                return response;
            }
        }

        public static async Task<ItProjectDTO> GetProjectAsync(int projectId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/itproject/{projectId}"), cookie))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItProjectDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendDeleteProjectAsync(int id, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var defaultOrganizationId = TestEnvironment.DefaultOrganizationId; //NOTE: Not even used in the endpoint
            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"api/itproject/{id}?organizationId={defaultOrganizationId}"), cookie);
        }

        public static async Task<HttpResponseMessage> SendChangeNameRequestAsync(int projectId, string newName, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/itproject/{projectId}?organizationId={organizationId}");
            var body = new
            {
                name = newName
            };
            using (var updatedResponse = await HttpApi.PatchWithCookieAsync(url, cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, updatedResponse.StatusCode);
                return updatedResponse;
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

        public static async Task<GoalDTO> AddGoalAsync(int organizationId, int projectId, string humanReadableId, bool measurable, string name, string description, DateTime goalDate1, DateTime goalDate2, DateTime goalDate3, Cookie optionalLogin = null)
        {
            using (var response = await SendAddGoalRequestAsync(organizationId, projectId, humanReadableId, measurable, name, description, goalDate1, goalDate2, goalDate3, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<GoalDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddGoalRequestAsync(int organizationId, int projectId, string humanReadableId, bool measurable, string name, string description, DateTime goalDate1, DateTime goalDate2, DateTime goalDate3, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/goal?organizationId={organizationId}");
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

        public static async Task<AssignmentDTO> AddAssignmentAsync(int organizationId, int projectId, string description, string name, string note, int statusPercentage, int timeEstimate, DateTime startDate, DateTime endDate, Cookie optionalLogin = null)
        {
            using (var response = await SendAddAssignmentRequestAsync(organizationId, projectId, description, name, note, statusPercentage, timeEstimate, startDate, endDate, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<AssignmentDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddAssignmentRequestAsync(int organizationId, int projectId, string description, string name, string note, int statusPercentage, int timeEstimate, DateTime startDate, DateTime endDate, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/assignment/?organizationId={organizationId}");
            var body = new
            {
                associatedItProjectId = projectId,
                associatedPhaseNum = 2,
                description = description,
                startDate = startDate,
                endDate = endDate,
                name = name,
                note = note,
                statusProcentage = statusPercentage, //yep spelling monkey on the left
                timeEstimate = timeEstimate
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<MilestoneDTO> AddMileStoneAsync(int organizationId, int projectId, string description, string name, string note, string humanReadableId, int timeEstimate, DateTime date, Cookie optionalLogin = null)
        {
            using (var response = await SendAddMileStoneRequestAsync(organizationId, projectId, description, name, note, humanReadableId, timeEstimate, date, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<MilestoneDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddMileStoneRequestAsync(int organizationId, int projectId, string description, string name, string note, string humanReadableId, int timeEstimate, DateTime date, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/milestone/?organizationId={organizationId}");
            var body = new
            {
                associatedItProjectId = projectId,
                associatedPhaseNum = 2,
                date = date,
                name = name,
                note = note,
                humanReadableId = humanReadableId,
                description = description,
                timeEstimate = timeEstimate,
                status = 2
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<StakeholderDTO> AddStakeholderAsync(int organizationId, int projectId, string name, string role, string downsides, string benefits, string howToHandle, int significance, Cookie optionalLogin = null)
        {
            using (var response = await SendAddStakeholderRequestAsync(organizationId, projectId, name, role, downsides, benefits, howToHandle, significance, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<StakeholderDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddStakeholderRequestAsync(int organizationId, int projectId, string name, string role, string downsides, string benefits, string howToHandle, int significance, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/stakeholder?organizationId={organizationId}");
            var body = new
            {
                benefits = benefits,
                downsides = downsides,
                howToHandle = howToHandle,
                itProjectId = projectId,
                name = name,
                role = role,
                significance = significance
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<RiskDTO> AddRiskAsync(int organizationId, int projectId, string name, string action, int consequence, int probability, int responsibleUserId, Cookie optionalLogin = null)
        {
            using (var response = await SendAddRiskRequestAsync(organizationId, projectId, name, action, consequence, probability, responsibleUserId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<RiskDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddRiskRequestAsync(int organizationId, int projectId, string name, string action, int consequence, int probability, int responsibleUserId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/risk?organizationId={organizationId}");
            var body = new
            {
                action = action,
                consequence = consequence,
                itProjectId = projectId,
                name = name,
                probability = probability,
                responsibleUserId = responsibleUserId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<CommunicationDTO> AddCommunicationAsync(int organizationId, int projectId, string media, string message, string purpose, int responsibleUserId, string targetAudience, DateTime dueDate, Cookie optionalLogin = null)
        {
            using (var response = await SendAddCommunicationRequestAsync(organizationId, projectId, media, message, purpose, responsibleUserId, targetAudience, dueDate, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<CommunicationDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddCommunicationRequestAsync(int organizationId, int projectId, string media, string message, string purpose, int responsibleUserId, string targetAudience, DateTime dueDate, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/communication?organizationId={organizationId}");
            var body = new
            {
                dueDate = dueDate,
                itProjectId = projectId,
                media = media,
                message = message,
                purpose = purpose,
                responsibleUserId = responsibleUserId,
                targetAudiance = targetAudience
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<HandoverDTO> AddHandoverResponsibleAsync(int handoverId, int responsibleUserId, Cookie optionalLogin = null)
        {
            using (var response = await SendAddHandoverResponsibleRequestAsync(handoverId, responsibleUserId, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<HandoverDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddHandoverResponsibleRequestAsync(int handoverId, int responsibleUserId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/handover/{handoverId}?participantId={responsibleUserId}");

            return await HttpApi.PostWithCookieAsync(url, cookie, new object());
        }
    }
}
