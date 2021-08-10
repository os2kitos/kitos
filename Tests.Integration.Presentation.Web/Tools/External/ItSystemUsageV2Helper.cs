using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Xunit;

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
            Guid? relationToSystemUsageUuidFilter = null)
        {
            var criteria = new List<KeyValuePair<string, string>>();

            if (organizationFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("organizationUuid", organizationFilter.Value.ToString("N")));

            if (systemUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("systemUuid", systemUuidFilter.Value.ToString("N")));

            if (relationToSystemUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToSystemUuid", relationToSystemUuidFilter.Value.ToString("N")));

            if (relationToSystemUsageUuidFilter.HasValue)
                criteria.Add(new KeyValuePair<string, string>("relatedToSystemUsageUuid", relationToSystemUsageUuidFilter.Value.ToString("N")));

            var joinedCriteria = string.Join("&", criteria.Select(x => $"{x.Key}={x.Value}"));
            var queryString = joinedCriteria.Any() ? $"?{joinedCriteria}" : string.Empty;
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-system-usages{queryString}"), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItSystemUsageResponseDTO>>();
        }
    }
}
