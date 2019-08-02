using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class HttpApi
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static Task<HttpResponseMessage> GetAsyncWithToken(Uri url, string token)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            return HttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> PostAsync(Uri url, object body)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            return HttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> GetAsync(Uri url)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            return HttpClient.SendAsync(requestMessage);
        }

        public static async Task<T> ReadResponseBodyAs<T>(this HttpResponseMessage response)
        {
            var responseAsJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseAsJson);
        }

        public static async Task<T> ReadResponseBodyAsKitosApiResponse<T>(this HttpResponseMessage response)
        {
            var apiReturnFormat = await response.ReadResponseBodyAs<ApiReturnDTO<T>>().ConfigureAwait(false);
            return apiReturnFormat.Response;
        }

        public static async Task<GetTokenResponseDTO> GetTokenAsync(OrganizationRole role)
        {
            var userCredentials = TestEnvironment.GetCredentials(role);
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");
            var loginDto = new LoginDTO
            {
                Email = userCredentials.Username,
                Password = userCredentials.Password
            };

            using (var httpResponseMessage = await HttpApi.PostAsync(url, loginDto))
            {
                Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
                var tokenResponse = await httpResponseMessage.ReadResponseBodyAsKitosApiResponse<GetTokenResponseDTO>().ConfigureAwait(false);

                Assert.Equal(loginDto.Email, tokenResponse.Email);
                Assert.True(tokenResponse.LoginSuccessful);
                Assert.True(tokenResponse.Expires > DateTime.UtcNow);
                Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token));

                return tokenResponse;
            }
        }
    }
}
