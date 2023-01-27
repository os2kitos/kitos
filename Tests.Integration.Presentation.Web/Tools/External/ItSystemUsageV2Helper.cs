using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.Shared;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Xunit;
using Xunit.Sdk;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ItSystemUsageV2Helper
    {
        public static async Task<ItSystemUsageResponseDTO> GetSingleAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid:D}"), token);
        }

        public static async Task<ResourcePermissionsResponseDTO> GetPermissionsAsync(string token, Guid uuid)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid:D}/permissions"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ResourcePermissionsResponseDTO>();
        }

        public static async Task<IEnumerable<ItSystemUsageResponseDTO>> GetManyAsync(
            string token,
            Guid? organizationFilter = null,
            Guid? systemUuidFilter = null,
            Guid? relationToSystemUuidFilter = null,
            Guid? relationToSystemUsageUuidFilter = null,
            Guid? relationToContractUuidFilter = null,
            string systemNameContentFilter = null,
            DateTime? changedSinceGtEq = null,
            int? page = null,
            int? pageSize = null)
        {
            var criteria = new List<KeyValuePair<string, string>>();

            if (organizationFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("organizationUuid", organizationFilter.Value.ToString()));

            if (systemUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("systemUuid", systemUuidFilter.Value.ToString()));

            if (relationToSystemUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToSystemUuid", relationToSystemUuidFilter.Value.ToString()));

            if (relationToSystemUsageUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToSystemUsageUuid", relationToSystemUsageUuidFilter.Value.ToString()));

            if (relationToContractUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToContractUuid", relationToContractUuidFilter.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(systemNameContentFilter))
                criteria.Add(new KeyValuePair<string, string>("systemNameContent", systemNameContentFilter));

            if (page.HasValue)
                criteria.Add(new KeyValuePair<string, string>("page", page.Value.ToString("D")));

            if (pageSize.HasValue)
                criteria.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (changedSinceGtEq.HasValue)
                criteria.Add(new KeyValuePair<string, string>("changedSinceGtEq", changedSinceGtEq.Value.ToString("O")));

            var joinedCriteria = string.Join("&", criteria.Select(x => $"{x.Key}={x.Value}"));
            var queryString = joinedCriteria.Any() ? $"?{joinedCriteria}" : string.Empty;
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages{queryString}"), token);

            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);


            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemUsageResponseDTO>>();
        }

        public static async Task<ItSystemUsageResponseDTO> PostAsync(string token, CreateItSystemUsageRequestDTO dto)
        {
            using var response = await SendPostAsync(token, dto);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPostAsync(string token, CreateItSystemUsageRequestDTO dto)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl("api/v2/it-system-usages"), dto, token);
        }

        public static async Task<ItSystemUsageResponseDTO> PutAsync(string token, Guid uuid, UpdateItSystemUsageRequestDTO dto)
        {
            using var response = await SendPutAsync(token, uuid, dto);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPutAsync(string token, Guid uuid, UpdateItSystemUsageRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPatchGeneral(string token, Guid uuid, GeneralDataUpdateRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.General)));
        }

        public static async Task<HttpResponseMessage> SendPatchOrganizationalUsage(string token, Guid uuid, OrganizationUsageWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage)));
        }

        public static async Task<HttpResponseMessage> SendPatchKle(string token, Guid uuid, LocalKLEDeviationsRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations)));
        }

        public static async Task<HttpResponseMessage> SendPatchExternalReferences(string token, Guid uuid, IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> payload)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, (payload ?? new List<UpdateExternalReferenceDataWriteRequestDTO>()).AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.ExternalReferences)));
        }

        public static async Task<HttpResponseMessage> SendPatchRoles(string token, Guid uuid, IEnumerable<RoleAssignmentRequestDTO> dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.Roles)));
        }

        public static async Task<HttpResponseMessage> SendPatchGDPR(string token, Guid uuid, GDPRWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.GDPR)));
        }

        public static async Task<HttpResponseMessage> SendPatchValidity(string token, Guid uuid, ItSystemUsageValidityWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto
                .AsPatchPayloadOfMultipleProperties(nameof(UpdateItSystemUsageRequestDTO.General), nameof(UpdateItSystemUsageRequestDTO.General.Validity)));
        }

        public static async Task<HttpResponseMessage> SendPatchArchiving(string token, Guid uuid, ArchivingWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.Archiving)));
        }

        public static async Task<HttpResponseMessage> SendDeleteAsync(string token, Guid uuid)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}"), token);
        }

        public static async Task<SystemRelationResponseDTO> PostRelationAsync(string token, Guid systemUsageUuid, SystemRelationWriteRequestDTO dto)
        {
            using var response = await SendPostRelationAsync(token, systemUsageUuid, dto);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<SystemRelationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPostRelationAsync(string token, Guid systemUsageUuid, SystemRelationWriteRequestDTO dto)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{systemUsageUuid}/system-relations"), dto, token);
        }

        public static async Task<SystemRelationResponseDTO> PutRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid, SystemRelationWriteRequestDTO dto)
        {
            using var response = await SendPutRelationAsync(token, systemUsageUuid, systemRelationUuid, dto);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<SystemRelationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPutRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid, SystemRelationWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{systemUsageUuid}/system-relations/{systemRelationUuid}"), token, dto);
        }

        public static async Task<SystemRelationResponseDTO> GetRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid)
        {
            using var response = await SendGetRelationAsync(token, systemUsageUuid, systemRelationUuid);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<SystemRelationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{systemUsageUuid}/system-relations/{systemRelationUuid}"), token);
        }

        public static async Task<HttpResponseMessage> SendDeleteRelationAsync(string token, Guid uuid, Guid systemRelationUuid)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}/system-relations/{systemRelationUuid}"), token);
        }

        public static async Task DeleteAsync(string token, Guid usageUuid)
        {
            using var response = await SendDeleteAsync(token, usageUuid);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
