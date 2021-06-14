using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.External.V2.Response;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.V2
{
    public static class ItSystemV2Helper
    {
        public static async Task<ItSystemResponseDTO> GetSingleAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-systems/{uuid:D}"), token);
        }

        public static async Task<IEnumerable<ItSystemResponseDTO>> GetManyAsync(string token, int? page = null, int? pageSize = null, Guid? rightsHolderId = null)
        {
            using var response = await SendGetManyAsync(token, page, pageSize, rightsHolderId);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetManyAsync(string token, int? page = null, int? pageSize = null, Guid? rightsHolderId = null)
        {
            var path = "api/v2/it-systems";
            var queryParameters = new List<KeyValuePair<string, string>>();

            if (page.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("page", page.Value.ToString("D")));

            if (pageSize.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (rightsHolderId.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("rightsHolderUuid", rightsHolderId.Value.ToString("D")));

            if (queryParameters.Any())
                path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
        }
    }
}
