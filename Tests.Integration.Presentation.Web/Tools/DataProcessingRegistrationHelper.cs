using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Models;
using Presentation.Web.Models.GDPR;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class DataProcessingRegistrationHelper
    {
        public static async Task<DataProcessingRegistrationDTO> CreateAsync(int organizationId, string name, Cookie optionalLogin = null)
        {
            using var createdResponse = await SendCreateRequestAsync(organizationId, name, optionalLogin);
            Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
            var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<DataProcessingRegistrationDTO>();

            Assert.Equal(name, response.Name);

            return response;
        }

        public static async Task<HttpResponseMessage> SendCreateRequestAsync(int organizationId, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new CreateDataProcessingRegistrationDTO
            {
                Name = name,
                OrganizationId = organizationId
            };

            return await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendDeleteRequestAsync(int id, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}"), cookie);
        }

        public static async Task<DataProcessingRegistrationDTO> GetAsync(int id, Cookie optionalLogin = null)
        {
            using var response = await SendGetRequestAsync(id, optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<DataProcessingRegistrationDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetRequestAsync(int id, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}"), cookie);
        }

        public static async Task<HttpResponseMessage> SendChangeNameRequestAsync(int id, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new SingleValueDTO<string> { Value = name };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/name"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendSetMasterReferenceRequestAsync(int id, int refId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new { Value = refId };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/master-reference"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendChangeOversightIntervalOptionRequestAsync(int id, YearMonthIntervalOption? oversightInterval, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new SingleValueDTO<YearMonthIntervalOption?> {Value = oversightInterval };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/oversight-option"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendChangeOversightIntervalOptionNoteRequestAsync(int id, string note, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new SingleValueDTO<string> { Value = note };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/oversight-option-note"), cookie, body);

        }

        public static async Task<HttpResponseMessage> SendCanCreateRequestAsync(int organizationId, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration?orgId={organizationId}&checkname={name}"), cookie);
        }

        public static async Task<IEnumerable<DataProcessingRegistrationReadModel>> QueryReadModelByNameContent(int organizationId, string nameContent, int top, int skip, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"odata/Organizations({organizationId})/DataProcessingRegistrationReadModels?$expand=RoleAssignments&$filter=contains(Name,'{nameContent}')&$top={top}&$skip={skip}&$orderBy=Name"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadOdataListResponseBodyAsAsync<DataProcessingRegistrationReadModel>();
        }

        public static async Task<IEnumerable<BusinessRoleDTO>> GetAvailableRolesAsync(int id, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/available-roles"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<BusinessRoleDTO>>();
        }

        public static async Task<IEnumerable<UserWithEmailDTO>> GetAvailableUsersAsync(int id, int roleId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/available-roles/{roleId}/applicable-users"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<UserWithEmailDTO>>();
        }

        public static async Task<HttpResponseMessage> SendAssignRoleRequestAsync(int id, int roleId, int userId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/roles/assign"), cookie,
                new AssignRoleDTO
                {
                    RoleId = roleId,
                    UserId = userId
                });
        }

        public static async Task<HttpResponseMessage> SendRemoveRoleRequestAsync(int id, int roleId, int userId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/roles/remove/{roleId}/from/{userId}"), cookie,
                new { });
        }

        public static async Task<IEnumerable<NamedEntityWithEnabledStatusDTO>> GetAvailableSystemsAsync(int id, string nameQuery, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/it-systems/available?nameQuery={nameQuery}"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<NamedEntityWithEnabledStatusDTO>>();
        }

        public static async Task<HttpResponseMessage> SendAssignSystemRequestAsync(int registrationId, int systemId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{registrationId}/it-systems/assign"), cookie, new { Value = systemId });
        }

        public static async Task<HttpResponseMessage> SendRemoveSystemRequestAsync(int registrationId, int systemId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{registrationId}/it-systems/remove"), cookie, new { Value = systemId });
        }

        public static async Task<IEnumerable<ShallowOrganizationDTO>> GetAvailableDataProcessors(int id, string nameQuery, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/data-processors/available?nameQuery={nameQuery}"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ShallowOrganizationDTO>>();
        }

        public static async Task<HttpResponseMessage> SendAssignDataProcessorRequestAsync(int registrationId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{registrationId}/data-processors/assign"), cookie, new SingleValueDTO<int> { Value = organizationId });
        }

        public static async Task<HttpResponseMessage> SendRemoveDataProcessorRequestAsync(int registrationId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{registrationId}/data-processors/remove"), cookie, new SingleValueDTO<int> { Value = organizationId });
        }

        public static async Task<IEnumerable<ShallowOrganizationDTO>> GetAvailableSubDataProcessors(int id, string nameQuery, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/sub-data-processors/available?nameQuery={nameQuery}"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ShallowOrganizationDTO>>();
        }

        public static async Task<HttpResponseMessage> SendAssignSubDataProcessorRequestAsync(int registrationId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{registrationId}/sub-data-processors/assign"), cookie, new SingleValueDTO<int> { Value = organizationId });
        }

        public static async Task<HttpResponseMessage> SendRemoveSubDataProcessorRequestAsync(int registrationId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{registrationId}/sub-data-processors/remove"), cookie, new SingleValueDTO<int> { Value = organizationId });
        }

        public static async Task<HttpResponseMessage> SendSetUseSubDataProcessorsStateRequestAsync(int registrationId, YesNoUndecidedOption value, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{registrationId}/sub-data-processors/state"), cookie, new SingleValueDTO<YesNoUndecidedOption> { Value = value });
		}

        public static async Task<HttpResponseMessage> SendChangeIsAgreementConcludedRequestAsync(int id, YesNoIrrelevantOption? yesNoIrrelevantOption, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new SingleValueDTO<YesNoIrrelevantOption?> { Value = yesNoIrrelevantOption };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/agreement-concluded"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendChangeAgreementConcludedAtRequestAsync(int id, DateTime? dateTime, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new SingleValueDTO<DateTime?> { Value = dateTime };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-registration/{id}/agreement-concluded-at"), cookie, body);
        }
    }
}
