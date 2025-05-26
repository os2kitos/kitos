using Presentation.Web.Models.API.V2.Response.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.DPR
{
    public class DataProcessingRegistrationInternalV2Helper
    {
        public static async Task<IEnumerable<DataProcessingRegistrationResponseDTO>> GetDPRsAsync(Cookie cookie, Guid organizationUuid, int page = 0, int pageSize = 10, string nameContains = null, DateTime? changedSinceGtEq = null)
        {
            using var response = await SendGetDPRsAsync(cookie, organizationUuid, page, pageSize, nameContains, changedSinceGtEq);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<DataProcessingRegistrationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetDPRsAsync(Cookie cookie, Guid organizationUuid, int page = 0, int pageSize = 10, string nameContains = null, DateTime? changedSinceGtEq = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
                new ("organizationUuid", organizationUuid.ToString("D"))
            };

            if(nameContains != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContains", nameContains));

            if (changedSinceGtEq.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("changedSinceGtEq", changedSinceGtEq.Value.ToString("O")));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));
            var path = TestEnvironment.CreateUrl($"api/v2/internal/data-processing-registrations/search?{query}");
            return await HttpApi.GetWithCookieAsync(path, cookie);
        }
    }
}
