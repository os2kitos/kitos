using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.External.V2.Response;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.V2
{
    public static class ItSystemV2Helper
    {
        public static async Task<ItSystemInResponseDto> GetSingleAsync(string token, Guid uuid)
        {
            using var response = await SendGetSingleAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemInResponseDto>();
        }

        public static async Task<HttpResponseMessage> SendGetSingleAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-systems/{uuid:D}"), token);
        }
    }
}
