using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Sensitive.Presentation.Web.Tools
{
    public class HttpApiSynch
    {

        private static readonly HttpClient HttpClient = new HttpClient();

        public static HttpResponseMessage Get(Uri url)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            return HttpClient.SendAsync(requestMessage).Result;
        }

        public static HttpResponseMessage Post(Uri url, object body)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            return HttpClient.SendAsync(requestMessage).Result;
        }

        public static HttpResponseMessage GetWithToken(Uri url, string token)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            return HttpClient.SendAsync(requestMessage).Result;
        }

        public static HttpResponseMessage PostWithToken(Uri url, object body, string tokenvalue)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"),
            };
            requestMessage.Headers.Add("Authorization", "bearer " + tokenvalue);

            return HttpClient.SendAsync(requestMessage).Result;
        }

        public static HttpResponseMessage DeleteWithCookie(Uri url, Cookie cookie)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Add("Cookie", cookie.Name + "=" + cookie.Value);
            return HttpClient.SendAsync(requestMessage).Result;
        }

        public static HttpResponseMessage PostWithCookie(Uri url, Cookie cookie, object body)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer.Add(cookie);
            HttpClient cookieClient = new HttpClient(handler);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            return cookieClient.SendAsync(requestMessage).Result;
        }

        public static GetTokenResponseDTO GetTokenAsync(OrganizationRole role)
        {
            var userCredentials = TestEnvironment.GetCredentials(role);
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");
            var loginDto = new LoginDTO
            {
                Email = userCredentials.Username,
                Password = userCredentials.Password
            };

            using (var httpResponseMessage = HttpApiSynch.Post(url, loginDto))
            {
                Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
                var tokenResponse = httpResponseMessage.ReadResponseBodyAsKitosApiResponse<GetTokenResponseDTO>().Result;

                Assert.Equal(loginDto.Email, tokenResponse.Email);
                Assert.True(tokenResponse.LoginSuccessful);
                Assert.True(tokenResponse.Expires > DateTime.UtcNow);
                Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token));

                return tokenResponse;
            }
        }

        public static Cookie GetCookieAsync(OrganizationRole role)
        {
            var userCredentials = TestEnvironment.GetCredentials(role);
            var url = TestEnvironment.CreateUrl("api/authorize");
            var loginDto = new LoginDTO
            {
                Email = userCredentials.Username,
                Password = userCredentials.Password
            };

            var cookieResponse = HttpApiSynch.Post(url, loginDto);
            var cookieParts = cookieResponse.Headers.Where(x => x.Key == "Set-Cookie").First().Value.First().Split('=');
            var cookieName = cookieParts[0];
            var cookieValue = cookieParts[1].Split(';')[0];

            return new Cookie(cookieName, cookieValue)
            {
                Domain = url.Host
            };

        }
    }
}
