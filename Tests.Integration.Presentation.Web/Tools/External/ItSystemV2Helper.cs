using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystem;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.System;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ItSystemV2Helper
    {
        public const string BasePath = "api/v2";
        public const string BaseRightsHolderPath = $"{BasePath}/rightsholder/it-systems";
        public const string BaseItSystemPath = $"{BasePath}/it-systems";
        public const string BaseItSystemInternalPath = $"{BasePath}/internal/it-systems";

        public static async Task<ItSystemResponseDTO> GetSingleAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/{uuid:D}"), token);
        }

        public static async Task<ItSystemResponseDTO> CreateSystemAsync(string token, CreateItSystemRequestDTO request)
        {
            using var response = await SendCreateSystemAsync(token, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendCreateSystemAsync(string token, CreateItSystemRequestDTO request)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl(BaseItSystemPath), request, token);
        }

        public static async Task<RightsHolderItSystemResponseDTO> CreateRightsHolderSystemAsync(string token, RightsHolderFullItSystemRequestDTO request)
        {
            using var response = await SendCreateRightsHolderSystemAsync(token, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendCreateRightsHolderSystemAsync(string token, RightsHolderFullItSystemRequestDTO request)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl(BaseRightsHolderPath), request, token);
        }

        public static async Task<RightsHolderItSystemResponseDTO> UpdateRightsHolderSystemAsync(string token, Guid uuid, RightsHolderFullItSystemRequestDTO request)
        {
            using var response = await SendUpdateRightsHolderSystemAsync(token, uuid, request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendUpdateRightsHolderSystemAsync(string token, Guid uuid, RightsHolderFullItSystemRequestDTO request)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"{BaseRightsHolderPath}/{uuid}"), token, request);
        }

        public static async Task<ItSystemResponseDTO> PatchSystemAsync(string token, Guid uuid, params KeyValuePair<string, object>[] changedProperties)
        {
            using var response = await SendPatchSystemAsync(token, uuid, changedProperties);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPatchSystemAsync(string token, Guid uuid, params KeyValuePair<string, object>[] changedProperties)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/{uuid}"), token, changedProperties.ToDictionary(x => x.Key, x => x.Value));
        }

        public static async Task<RightsHolderItSystemResponseDTO> PatchRightsHolderSystemAsync(string token, Guid uuid, params KeyValuePair<string, object>[] changedProperties)
        {
            using var response = await SendPatchUpdateRightsHolderSystemAsync(token, uuid, changedProperties);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPatchUpdateRightsHolderSystemAsync(string token, Guid uuid, params KeyValuePair<string, object>[] changedProperties)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{BaseRightsHolderPath}/{uuid}"), token, changedProperties.ToDictionary(x => x.Key, x => x.Value));
        }

        public static async Task<HttpResponseMessage> SendDeleteSystemAsync(string token, Guid uuid)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/{uuid}"), token);
        }

        public static async Task<HttpResponseMessage> SendDeleteRightsHolderSystemAsync(string token, Guid uuid, DeactivationReasonRequestDTO request)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"{BaseRightsHolderPath}/{uuid}"), token, request);
        }

        public static async Task<RightsHolderItSystemResponseDTO> GetSingleRightsHolderSystemAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleRightsHolderSystemAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleRightsHolderSystemAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{BaseRightsHolderPath}/{uuid:D}"), token);
        }

        public static async Task<IEnumerable<ItSystemSearchResponseDTO>> GetManyInternalAsync(
            Cookie cookie,
            int? page = null,
            int? pageSize = null,
            Guid? rightsHolderId = null,
            Guid? businessTypeId = null,
            string kleKey = null,
            Guid? kleUuid = null,
            int? numberOfUsers = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            string nameEquals = null,
            string nameContains = null
        )
        {
            using var response = await SendGetManyAsync($"{BaseItSystemInternalPath}/search",
                page,
                pageSize,
                rightsHolderId,
                businessTypeId,
                kleKey,
                kleUuid,
                numberOfUsers,
                includeDeactivated,
                changedSinceGtEq,
                nameEquals: nameEquals,
                nameContains: nameContains,
                cookie: cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemSearchResponseDTO>>();
        }

        public static async Task<IEnumerable<ItSystemResponseDTO>> GetManyAsync(
            string token,
            int? page = null,
            int? pageSize = null,
            Guid? rightsHolderId = null,
            Guid? businessTypeId = null,
            string kleKey = null,
            Guid? kleUuid = null,
            int? numberOfUsers = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            Guid? usedInOrganizationUuid = null,
            string nameContains = null
            )
        {
            using var response = await SendGetManyAsync(BaseItSystemPath,
                page,
                pageSize,
                rightsHolderId,
                businessTypeId,
                kleKey,
                kleUuid,
                numberOfUsers,
                includeDeactivated,
                changedSinceGtEq,
                usedInOrganizationUuid,
                nameContains: nameContains,
                token: token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetManyAsync(
            string path,
            int? page = null,
            int? pageSize = null,
            Guid? rightsHolderId = null,
            Guid? businessTypeId = null,
            string kleKey = null,
            Guid? kleUuid = null,
            int? numberOfUsers = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            Guid? usedInOrganizationUuid = null,
            string nameEquals = null,
            string nameContains = null,
            string token = null,
            Cookie cookie = null
            )
        {
            var queryParameters = new List<KeyValuePair<string, string>>();

            if (page.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("page", page.Value.ToString("D")));

            if (pageSize.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (rightsHolderId.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("rightsHolderUuid", rightsHolderId.Value.ToString("D")));

            if (businessTypeId.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("businessTypeUuid", businessTypeId.Value.ToString("D")));

            if (kleKey != null)
                queryParameters.Add(new KeyValuePair<string, string>("kleNumber", kleKey));

            if (kleUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("kleUuid", kleUuid.Value.ToString("D")));


            if (numberOfUsers.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("numberOfUsers", numberOfUsers.Value.ToString("D")));

            if (includeDeactivated.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("includeDeactivated", includeDeactivated.Value.ToString()));

            if (changedSinceGtEq.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("changedSinceGtEq", changedSinceGtEq.Value.ToString("O")));

            if (usedInOrganizationUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("usedInOrganizationUuid", usedInOrganizationUuid.Value.ToString("D")));

            if (nameEquals != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameEquals", nameEquals));

            if (nameContains != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContains", nameContains));

            if (queryParameters.Any())
                path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            if (token != null)
                return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
            Assert.NotNull(cookie);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl(path), cookie);
        }

        public static async Task<IEnumerable<ItSystemResponseDTO>> GetManyRightsHolderSystemsAsync(
            string token,
            int? page = null,
            int? pageSize = null,
            Guid? rightsHolderUuid = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null)
        {
            using var response = await SendGetManyRightsHolderSystemsAsync(token, page, pageSize, rightsHolderUuid, includeDeactivated, changedSinceGtEq);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetManyRightsHolderSystemsAsync(
            string token,
            int? page = null,
            int? pageSize = null,
            Guid? rightsHolderUuid = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null)
        {
            var path = BaseRightsHolderPath;
            var queryParameters = new List<KeyValuePair<string, string>>();

            if (page.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("page", page.Value.ToString("D")));

            if (pageSize.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (rightsHolderUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("rightsHolderUuid", rightsHolderUuid.Value.ToString("D")));

            if (includeDeactivated.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("includeDeactivated", includeDeactivated.Value.ToString()));

            if (changedSinceGtEq.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("changedSinceGtEq", changedSinceGtEq.Value.ToString("O")));

            if (queryParameters.Any())
                path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
        }

        public static async Task<IEnumerable<RegistrationHierarchyNodeWithActivationStatusResponseDTO>> GetHierarchyAsync(string token, Guid systemUuid)
        {
            var path = $"{BaseItSystemPath}/{systemUuid}/hierarchy";
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<RegistrationHierarchyNodeWithActivationStatusResponseDTO>>();
        }
        public static async Task<IEnumerable<ItSystemHierarchyNodeResponseDTO>> GetInternalHierarchyAsync(Guid organizationUuid, Guid systemUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var path = $"{BasePath}/internal/organization/{organizationUuid}/it-systems/{systemUuid}/hierarchy";
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl(path), cookie);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemHierarchyNodeResponseDTO>>();
        }

        public static async Task<ExternalReferenceDataResponseDTO> AddExternalReferenceAsync(string token, Guid systemUuid, ExternalReferenceDataWriteRequestDTO request)
        {
            using var response = await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/{systemUuid}/external-references"), request, token);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ExternalReferenceDataResponseDTO>();
        }

        public static async Task<ExternalReferenceDataResponseDTO> UpdateExternalReferenceAsync(string token, Guid systemUuid, Guid externalReferenceUuid, ExternalReferenceDataWriteRequestDTO request)
        {
            using var response = await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/{systemUuid}/external-references/{externalReferenceUuid}"), token, request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ExternalReferenceDataResponseDTO>();
        }

        public static async Task DeleteExternalReferenceAsync(string token, Guid systemUuid, Guid externalReferenceUuid)
        {
            using var response = await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/{systemUuid}/external-references/{externalReferenceUuid}"), token);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static async Task<ItSystemPermissionsResponseDTO> GetPermissionsAsync(string token, Guid uuid)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/{uuid:D}/permissions"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemPermissionsResponseDTO>();
        }

        public static async Task<ResourceCollectionPermissionsResponseDTO> GetCollectionPermissionsAsync(string token, Guid organizationUuid)
        {
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{BaseItSystemPath}/permissions?organizationUuid={organizationUuid:D}"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ResourceCollectionPermissionsResponseDTO>();
        }
    }
}
