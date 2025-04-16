using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ItSystemUsageV2Helper
    {
        private const string _baseUsageApiPath = "api/v2/it-system-usages";
        private const string _baseUsageInternalApiPath = "api/v2/internal/it-system-usages";

        public static async Task<IEnumerable<ExtendedRoleAssignmentResponseDTO>> GetRoleAssignmentsInternalAsync(Guid uuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await SendGetRoleAssignmentsInternalRequestAsync(cookie, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ExtendedRoleAssignmentResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetRoleAssignmentsInternalRequestAsync(Cookie cookie, Guid uuid)
        {
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{_baseUsageInternalApiPath}/{uuid:D}/roles"), cookie);
        }

        public static async Task<ItSystemUsageResponseDTO> GetSingleAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid:D}"), token);
        }

        public static async Task<ResourcePermissionsResponseDTO> GetPermissionsAsync(string token, Guid uuid)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid:D}/permissions"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ResourcePermissionsResponseDTO>();
        }

        public static async Task<ResourceCollectionPermissionsResponseDTO> GetCollectionPermissionsAsync(string token, Guid organizationUuid)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/permissions?organizationUuid={organizationUuid:D}"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ResourceCollectionPermissionsResponseDTO>();
        }

        public static async Task<IEnumerable<ItSystemUsageSearchResultResponseDTO>> GetManyInternalAsync(
            Guid organizationFilter,
            Guid? systemUuidFilter = null,
            Guid? relationToSystemUuidFilter = null,
            Guid? relationToSystemUsageUuidFilter = null,
            Guid? relationToContractUuidFilter = null,
            string systemNameContentFilter = null,
            DateTime? changedSinceGtEq = null,
            int? page = null,
            int? pageSize = null)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await QueryItSystemUsages($"{_baseUsageInternalApiPath}/search", organizationFilter, systemUuidFilter, relationToSystemUuidFilter, relationToSystemUsageUuidFilter, relationToContractUuidFilter, systemNameContentFilter, changedSinceGtEq, page, pageSize,cookie:cookie);

            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);


            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemUsageSearchResultResponseDTO>>();
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
            using var response = await QueryItSystemUsages(_baseUsageApiPath, organizationFilter, systemUuidFilter, relationToSystemUuidFilter, relationToSystemUsageUuidFilter, relationToContractUuidFilter, systemNameContentFilter, changedSinceGtEq, page, pageSize, token);

            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);


            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemUsageResponseDTO>>();
        }

        private static async Task<HttpResponseMessage> QueryItSystemUsages(string basePath, Guid? organizationFilter, Guid? systemUuidFilter,
            Guid? relationToSystemUuidFilter, Guid? relationToSystemUsageUuidFilter, Guid? relationToContractUuidFilter,
            string systemNameContentFilter, DateTime? changedSinceGtEq, int? page, int? pageSize, string token = null, Cookie cookie = null)
        {
            var criteria = new List<KeyValuePair<string, string>>();

            if (organizationFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("organizationUuid", organizationFilter.Value.ToString()));

            if (systemUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("systemUuid", systemUuidFilter.Value.ToString()));

            if (relationToSystemUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToSystemUuid",
                    relationToSystemUuidFilter.Value.ToString()));

            if (relationToSystemUsageUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToSystemUsageUuid",
                    relationToSystemUsageUuidFilter.Value.ToString()));

            if (relationToContractUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToContractUuid",
                    relationToContractUuidFilter.Value.ToString()));

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

            if (token != null)
            {
                return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{basePath}{queryString}"), token);
            }
            Assert.NotNull(cookie); //if no token, a cookie must be provided
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{basePath}{queryString}"), cookie);
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
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl(_baseUsageApiPath), dto, token);
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
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPatchGeneral(string token, Guid uuid, GeneralDataUpdateRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.General)));
        }

        public static async Task<HttpResponseMessage> SendPatchOrganizationalUsage(string token, Guid uuid, OrganizationUsageWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage)));
        }

        public static async Task<HttpResponseMessage> SendPatchKle(string token, Guid uuid, LocalKLEDeviationsRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations)));
        }

        public static async Task<HttpResponseMessage> SendPatchExternalReferences(string token, Guid uuid, IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> payload)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, (payload ?? new List<UpdateExternalReferenceDataWriteRequestDTO>()).AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.ExternalReferences)));
        }

        public static async Task<HttpResponseMessage> SendPatchRoles(string token, Guid uuid, IEnumerable<RoleAssignmentRequestDTO> dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.Roles)));
        }

        public static async Task<HttpResponseMessage> SendPatchAddRoleAssignment(string token, Guid uuid, RoleAssignmentRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}/roles/add"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPatchAddBulkRoleAssignment(string token, Guid uuid, BulkRoleAssignmentRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}/roles/bulk/add"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPatchRemoveRoleAssignment(string token, Guid uuid, RoleAssignmentRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{uuid}/roles/remove"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPatchGDPR(string token, Guid uuid, GDPRWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.GDPR)));
        }

        public static async Task<HttpResponseMessage> SendPatchValidity(string token, Guid uuid, ItSystemUsageValidityWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto
                .AsPatchPayloadOfMultipleProperties(nameof(UpdateItSystemUsageRequestDTO.General), nameof(UpdateItSystemUsageRequestDTO.General.Validity)));
        }

        public static async Task<HttpResponseMessage> SendPatchArchiving(string token, Guid uuid, BaseArchivingWriteRequestDTO dto)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token, dto.AsPatchPayloadOfProperty(nameof(UpdateItSystemUsageRequestDTO.Archiving)));
        }

        public static async Task<HttpResponseMessage> SendDeleteAsync(string token, Guid uuid)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}"), token);
        }

        public static async Task<OutgoingSystemRelationResponseDTO> PostRelationAsync(string token, Guid systemUsageUuid, SystemRelationWriteRequestDTO dto)
        {
            using var response = await SendPostRelationAsync(token, systemUsageUuid, dto);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OutgoingSystemRelationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPostRelationAsync(string token, Guid systemUsageUuid, SystemRelationWriteRequestDTO dto)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{systemUsageUuid}/system-relations"), dto, token);
        }

        public static async Task<OutgoingSystemRelationResponseDTO> PutRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid, SystemRelationWriteRequestDTO dto)
        {
            using var response = await SendPutRelationAsync(token, systemUsageUuid, systemRelationUuid, dto);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OutgoingSystemRelationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPutRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid, SystemRelationWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{systemUsageUuid}/system-relations/{systemRelationUuid}"), token, dto);
        }

        public static async Task<OutgoingSystemRelationResponseDTO> GetRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid)
        {
            using var response = await SendGetRelationAsync(token, systemUsageUuid, systemRelationUuid);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<OutgoingSystemRelationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetRelationAsync(string token, Guid systemUsageUuid, Guid systemRelationUuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{systemUsageUuid}/system-relations/{systemRelationUuid}"), token);
        }

        public static async Task<IEnumerable<IncomingSystemRelationResponseDTO>> GetIncomingRelationsAsync(string token, Guid systemUsageUuid)
        {
            using var response = await SendGetIncomingRelationsAsync(token, systemUsageUuid);
            if (!response.IsSuccessStatusCode)
                Debug.WriteLine(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<IncomingSystemRelationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetIncomingRelationsAsync(string token, Guid systemUsageUuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages/{systemUsageUuid}/incoming-system-relations"), token);
        }

        public static async Task<HttpResponseMessage> SendDeleteRelationAsync(string token, Guid uuid, Guid systemRelationUuid)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{uuid}/system-relations/{systemRelationUuid}"), token);
        }

        public static async Task DeleteAsync(string token, Guid usageUuid)
        {
            using var response = await SendDeleteAsync(token, usageUuid);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static async Task<ExternalReferenceDataResponseDTO> AddExternalReferenceAsync(string token, Guid usageUuid, ExternalReferenceDataWriteRequestDTO request)
        {
            using var response = await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{usageUuid}/external-references"), request, token);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ExternalReferenceDataResponseDTO>();
        }

        public static async Task<ExternalReferenceDataResponseDTO> UpdateExternalReferenceAsync(string token, Guid usageUuid, Guid externalReferenceUuid, ExternalReferenceDataWriteRequestDTO request)
        {
            using var response = await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{usageUuid}/external-references/{externalReferenceUuid}"), token, request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ExternalReferenceDataResponseDTO>();
        }

        public static async Task DeleteExternalReferenceAsync(string token, Guid usageUuid, Guid externalReferenceUuid)
        {
            using var response = await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"{_baseUsageApiPath}/{usageUuid}/external-references/{externalReferenceUuid}"), token);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static async Task<JournalPeriodResponseDTO> CreateJournalPeriodAsync(string token, Guid systemUsageUuid, JournalPeriodDTO input)
        {
            var path = $"{_baseUsageApiPath}/{systemUsageUuid}/journal-periods";
            using var response = await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl(path), input, token);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<JournalPeriodResponseDTO>();
        }

        public static async Task<JournalPeriodResponseDTO> GetJournalPeriodAsync(string token, Guid systemUsageUuid, Guid journalPeriodUuid)
        {
            using var response = await SendGetJournalPeriodAsync(token, systemUsageUuid, journalPeriodUuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<JournalPeriodResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetJournalPeriodAsync(string token, Guid systemUsageUuid, Guid journalPeriodUuid)
        {
            var path = $"{_baseUsageApiPath}/{systemUsageUuid}/journal-periods/{journalPeriodUuid}";
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
        }

        public static async Task<JournalPeriodResponseDTO> UpdateJournalPeriodAsync(string token, Guid systemUsageUuid, Guid journalPeriodUuid, JournalPeriodDTO input)
        {
            var path = $"{_baseUsageApiPath}/{systemUsageUuid}/journal-periods/{journalPeriodUuid}";
            using var response = await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl(path), token, input);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<JournalPeriodResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendDeleteJournalPeriodAsync(string token, Guid systemUsageUuid, Guid journalPeriodUuid)
        {
            var path = $"{_baseUsageApiPath}/{systemUsageUuid}/journal-periods/{journalPeriodUuid}";
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl(path), token);
        }
    }
}
