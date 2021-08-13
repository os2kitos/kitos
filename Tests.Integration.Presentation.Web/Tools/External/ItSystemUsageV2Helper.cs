using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
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

        public static async Task<IEnumerable<ItSystemUsageResponseDTO>> GetManyAsync(
            string token,
            Guid? organizationFilter = null,
            Guid? systemUuidFilter = null,
            Guid? relationToSystemUuidFilter = null,
            Guid? relationToSystemUsageUuidFilter = null,
            string systemNameContentFilter = null,
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

            if (!string.IsNullOrWhiteSpace(systemNameContentFilter))
                criteria.Add(new KeyValuePair<string, string>("systemNameContent", systemNameContentFilter));

            if (page.HasValue)
                criteria.Add(new KeyValuePair<string, string>("page", page.Value.ToString("D")));

            if (pageSize.HasValue)
                criteria.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

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
    }
}
