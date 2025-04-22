using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Options;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class DataProcessingRegistrationV2Helper
    {
        public static async Task<IEnumerable<DataProcessingRegistrationResponseDTO>> GetDPRsAsync(string token, int page = 0, int pageSize = 10, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessorUuid = null, Guid? subDataProcessorUuid = null, bool? agreementConcluded = null, string nameContains = null, string nameEquals = null, DateTime? changedSinceGtEq = null)
        {
            using var response = await SendGetDPRsAsync(token, page, pageSize, organizationUuid, systemUuid, systemUsageUuid, dataProcessorUuid, subDataProcessorUuid, agreementConcluded, nameContains, nameEquals, changedSinceGtEq);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<DataProcessingRegistrationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetDPRsAsync(string token, int page = 0, int pageSize = 10, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessorUuid = null, Guid? subDataProcessorUuid = null, bool? agreementConcluded = null, string nameContains = null, string nameEquals = null, DateTime? changedSinceGtEq = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            if (organizationUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("organizationUuid", organizationUuid.Value.ToString("D")));

            if (systemUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUuid", systemUuid.Value.ToString("D")));

            if (systemUsageUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUsageUuid", systemUsageUuid.Value.ToString("D")));

            if (dataProcessorUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("dataProcessorUuid", dataProcessorUuid.Value.ToString("D")));

            if (subDataProcessorUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("subDataProcessorUuid", subDataProcessorUuid.Value.ToString("D")));

            if (agreementConcluded.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("agreementConcluded", agreementConcluded.Value.ToString()));

            if (nameContains != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContains", nameContains));

            if (nameEquals != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameEquals", nameEquals));

            if (changedSinceGtEq.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("changedSinceGtEq", changedSinceGtEq.Value.ToString("O")));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations?{query}"), token);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> GetDPRAsync(string token, Guid uuid)
        {
            using var response = await SendGetDPRAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetDPRAsync(string token, Guid uuid, bool withIntegerEnums = false)
        {
            var headers = new List<KeyValuePair<string, string>>
            {
                new(KitosConstants.Headers.SerializeEnumAsInteger, withIntegerEnums ? bool.TrueString : bool.FalseString)
            };
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid:D}"), token, headers);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> PostAsync(string token, CreateDataProcessingRegistrationRequestDTO payload)
        {
            using var response = await SendPostAsync(token, payload);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPostAsync(string token, CreateDataProcessingRegistrationRequestDTO payload)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations"), payload, token);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> PutAsync(string token, Guid uuid, UpdateDataProcessingRegistrationRequestDTO payload)
        {
            using var response = await SendPutAsync(token, uuid, payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPutAsync(string token, Guid uuid, UpdateDataProcessingRegistrationRequestDTO payload)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, payload);
        }

        public static async Task<HttpResponseMessage> SendPatchGeneralDataAsync(string token, Guid uuid, DataProcessingRegistrationGeneralDataWriteRequestDTO payload)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, CreatePatchPayload(nameof(DataProcessingRegistrationWriteRequestDTO.General), payload));
        }

        public static async Task<HttpResponseMessage> SendPatchSystemsAsync(string token, Guid uuid, IEnumerable<Guid> payload)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, CreatePatchPayload(nameof(DataProcessingRegistrationWriteRequestDTO.SystemUsageUuids), payload));
        }

        public static async Task DeleteAsync(string token, Guid uuid)
        {
            using var response = await SendDeleteAsync(token, uuid);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        public static async Task<HttpResponseMessage> SendDeleteAsync(string token, Guid uuid)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> PatchOversightAsync(string token, Guid uuid, DataProcessingRegistrationOversightWriteRequestDTO payload)
        {
            using var response = await SendPatchOversightAsync(token, uuid, payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPatchOversightAsync(string token, Guid uuid, DataProcessingRegistrationOversightWriteRequestDTO payload)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, CreatePatchPayload(nameof(DataProcessingRegistrationWriteRequestDTO.Oversight), payload));
        }

        public static async Task<HttpResponseMessage> SendPatchRolesAsync(string token, Guid uuid, IEnumerable<RoleAssignmentRequestDTO> payload)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, CreatePatchPayload(nameof(DataProcessingRegistrationWriteRequestDTO.Roles), payload));
        }

        public static async Task<HttpResponseMessage> SendPatchExternalReferences(string token, Guid uuid, IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> payload)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, CreatePatchPayload(nameof(UpdateDataProcessingRegistrationRequestDTO.ExternalReferences), payload));
        }

        public static async Task<IEnumerable<RoleOptionResponseDTO>> GetRolesAsync(string token, Guid organizationUuid, int page = 0, int pageSize = 10)
        {
            using var response = await SendGetRolesAsync(token, organizationUuid, page, pageSize);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<RoleOptionResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetRolesAsync(string token, Guid organizationUuid, int page = 0, int pageSize = 10)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            queryParameters.Add(new KeyValuePair<string, string>("organizationUuid", organizationUuid.ToString("D")));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registration-role-types?{query}"), token);
        }

        private static Dictionary<string, object> CreatePatchPayload(string propertyName, object dto)
        {
            return dto.AsPatchPayloadOfProperty(propertyName);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> PatchNameAsync(string token, Guid uuid, string name)
        {
            using var response = await SendPatchName(token, uuid, name);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPatchName(string token, Guid uuid, string name)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, CreatePatchPayload(nameof(UpdateDataProcessingRegistrationRequestDTO.Name), name));
        }

        public static async Task<DataProcessingRegistrationPermissionsResponseDTO> GetPermissionsAsync(string token, Guid uuid)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid:D}/permissions"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationPermissionsResponseDTO>();
        }

        public static async Task<ResourceCollectionPermissionsResponseDTO> GetCollectionPermissionsAsync(string token, Guid organizationUuid)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/permissions?organizationUuid={organizationUuid:D}"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ResourceCollectionPermissionsResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPatchAddRoleAssignment(Guid uuid, RoleAssignmentRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var response = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/data-processing-registrations/{uuid}/roles/add"), cookie, dto);
            return response;
        }

        public static async Task<HttpResponseMessage> SendPatchAddBulkRoleAssignment(Guid uuid, BulkRoleAssignmentRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var response = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/data-processing-registrations/{uuid}/roles/bulk/add"), cookie, dto);
            return response;
        }

        public static async Task<HttpResponseMessage> SendPatchRemoveRoleAssignment(Guid uuid, RoleAssignmentRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/data-processing-registrations/{uuid}/roles/remove"), cookie, dto);
        }

        public static async Task<IEnumerable<ShallowOrganizationResponseDTO>> GetAvailableDataProcessors(Guid uuid, string nameQuery, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/data-processing-registrations/{uuid}/data-processors/available?nameQuery={nameQuery}"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<ShallowOrganizationResponseDTO>>();
        }

        public static async Task<IEnumerable<ShallowOrganizationResponseDTO>> GetAvailableSubDataProcessors(Guid uuid, string nameQuery, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/data-processing-registrations/{uuid}/sub-data-processors/available?nameQuery={nameQuery}"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<ShallowOrganizationResponseDTO>>();
        }

        public static async Task<IEnumerable<IdentityNamePairResponseDTO>> GetAvailableSystemsAsync(Guid uuid, string nameQuery, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/data-processing-registrations/{uuid}/system-usages/available?nameQuery={nameQuery}"), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<IdentityNamePairResponseDTO>>();
        }
    }
}