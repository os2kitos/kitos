using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class DataProcessingRegistrationV2Helper
    {
        public static async Task<IEnumerable<IdentityNamePairResponseDTO>> GetDPRsAsync(string token, int page = 0, int pageSize = 10, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null)
        {
            using var response = await SendGetDPRsAsync(token, page, pageSize, organizationUuid, systemUuid, systemUsageUuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<IdentityNamePairResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetDPRsAsync(string token, int page = 0, int pageSize = 10, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            if(organizationUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("organizationUuid", organizationUuid.Value.ToString("D")));

            if (systemUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUuid", systemUuid.Value.ToString("D")));

            if (systemUsageUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUsageUuid", systemUsageUuid.Value.ToString("D")));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations?{query}"), token);
        }

        public static async Task<IdentityNamePairResponseDTO> GetDPRAsync(string token, Guid uuid)
        {
            using var response = await SendGetDPRAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IdentityNamePairResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetDPRAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid:D}"), token);
        }
    }
}