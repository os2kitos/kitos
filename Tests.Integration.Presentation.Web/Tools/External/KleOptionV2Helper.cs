using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Response.KLE;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class KleOptionV2Helper
    {
        public static async Task<VersionedKLEResponseDTO<IEnumerable<KLEDetailsDTO>>> GetKleNumbersAsync(
            string token,
            int page = 0,
            int pageSize = 100,
            Guid? parentIdQuery = null,
            string parentKeyQuery = null,
            string kleNumberPrefix = null,
            string kleDescriptionContent = null)
        {
            using var response = await SendGetKleNumbersAsync(token, page, pageSize, parentIdQuery, parentKeyQuery, kleNumberPrefix, kleDescriptionContent);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<VersionedKLEResponseDTO<IEnumerable<KLEDetailsDTO>>>();
        }

        public static async Task<HttpResponseMessage> SendGetKleNumbersAsync(string token,
            int page = 0,
            int pageSize = 100,
            Guid? parentIdQuery = null,
            string parentKeyQuery = null,
            string kleNumberPrefix = null,
            string kleDescriptionContent = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            if (parentIdQuery.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("parentKleUuid", parentIdQuery.Value.ToString("D")));

            if (parentKeyQuery != null)
                queryParameters.Add(new KeyValuePair<string, string>("parentKleNumber", parentKeyQuery));

            if (kleNumberPrefix != null)
                queryParameters.Add(new KeyValuePair<string, string>("kleNumberPrefix", kleNumberPrefix));

            if (kleDescriptionContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("kleDescriptionContent", kleDescriptionContent));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            var url = TestEnvironment.CreateUrl($"/api/v2/kle-options?{query}");
            return await HttpApi.GetWithTokenAsync(url, token);
        }

        public static async Task<VersionedKLEResponseDTO<KLEDetailsDTO>> GetKleNumberAsync(string token, Guid uuid)
        {
            using var response = await SendGetKleNumberAsync(token, uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<VersionedKLEResponseDTO<KLEDetailsDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetKleNumberAsync(string token, Guid uuid)
        {
            var url = TestEnvironment.CreateUrl($"/api/v2/kle-options/{uuid}");
            return await HttpApi.GetWithTokenAsync(url, token);
        }
    }
}
