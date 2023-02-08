using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.System;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;
using Presentation.Web.Models.API.V2.Response.System;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ItSystemV2Helper
    {
        private const string _basePath = "api/v2";

        private const string _baseRightsHolderPath = $"{_basePath}/rightsholder/it-systems";
        private const string _baseItSystemPath = $"{_basePath}/it-systems";

        public static async Task<ItSystemResponseDTO> GetSingleAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{_baseItSystemPath}/{uuid:D}"), token);
        }

        public static async Task<RightsHolderItSystemResponseDTO> CreateRightsHolderSystemAsync(string token, RightsHolderCreateItSystemRequestDTO request)
        {
            using var response = await SendCreateRightsHolderSystemAsync(token, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendCreateRightsHolderSystemAsync(string token, RightsHolderCreateItSystemRequestDTO request)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl(_baseRightsHolderPath), request, token);
        }

        public static async Task<RightsHolderItSystemResponseDTO> UpdateRightsHolderSystemAsync(string token, Guid uuid, RightsHolderWritableITSystemPropertiesDTO request)
        {
            using var response = await SendUpdateRightsHolderSystemAsync(token, uuid, request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendUpdateRightsHolderSystemAsync(string token, Guid uuid, RightsHolderWritableITSystemPropertiesDTO request)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"{_baseRightsHolderPath}/{uuid}"), token, request);
        }

        public static async Task<RightsHolderItSystemResponseDTO> PatchRightsHolderSystemAsync(string token, Guid uuid, params KeyValuePair<string, object>[] changedProperties)
        {
            using var response = await SendPatchUpdateRightsHolderSystemAsync(token, uuid, changedProperties);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPatchUpdateRightsHolderSystemAsync(string token, Guid uuid, params KeyValuePair<string, object>[] changedProperties)
        {
            return await HttpApi.PatchWithTokenAsync(TestEnvironment.CreateUrl($"{_baseRightsHolderPath}/{uuid}"), token, changedProperties.ToDictionary(x => x.Key, x => x.Value));
        }

        public static async Task<HttpResponseMessage> SendDeleteRightsHolderSystemAsync(string token, Guid uuid, DeactivationReasonRequestDTO request)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"{_baseRightsHolderPath}/{uuid}"), token, request);
        }

        public static async Task<RightsHolderItSystemResponseDTO> GetSingleRightsHolderSystemAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleRightsHolderSystemAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleRightsHolderSystemAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{_baseRightsHolderPath}/{uuid:D}"), token);
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
            DateTime? changedSinceGtEq = null
            )
        {
            using var response = await SendGetManyAsync(token, page, pageSize, rightsHolderId, businessTypeId, kleKey, kleUuid, numberOfUsers, includeDeactivated, changedSinceGtEq);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetManyAsync(
            string token,
            int? page = null,
            int? pageSize = null,
            Guid? rightsHolderId = null,
            Guid? businessTypeId = null,
            string kleKey = null,
            Guid? kleUuid = null,
            int? numberOfUsers = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null
            )
        {
            var path = _baseItSystemPath;
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

            if (queryParameters.Any())
                path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
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
            var path = _baseRightsHolderPath;
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

        public static async Task<IEnumerable<RegistrationHierarchyNodeResponseDTO>> GetHierarchyAsync(string token, Guid systemUuid)
        {
            var path = $"{_baseItSystemPath}/{systemUuid}/hierarchy";
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<RegistrationHierarchyNodeResponseDTO>>();
        }
    }
}
