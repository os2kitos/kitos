using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class HttpApi
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static Task<HttpResponseMessage> GetWithTokenAsync(Uri url, string token)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            return HttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> PostWithCookieAsync(Uri url, Cookie cookie, object body)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Cookie", cookie.Name + "=" + cookie.Value);

            return HttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> DeleteWithCookieAsync(Uri url, Cookie cookie)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Add("Cookie", cookie.Name + "=" + cookie.Value);
            return HttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> PatchWithCookieAsync(Uri url, Cookie cookie, object body)
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Cookie", cookie.Name + "=" + cookie.Value);
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

        public static Task<HttpResponseMessage> PostWithTokenAsync(Uri url, object body, string tokenvalue)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"),
            };
            requestMessage.Headers.Add("Authorization", "bearer " + tokenvalue);

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

        public static async Task<List<T>> ReadOdataListResponseBodyAs<T>(this HttpResponseMessage response)
        {
            var responseAsJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var spec = new {value = new List<T>()};
            var result = JsonConvert.DeserializeAnonymousType(responseAsJson, spec);
            return result.value;
        }

        public static async Task<T> ReadResponseBodyAsKitosApiResponse<T>(this HttpResponseMessage response)
        {
            var apiReturnFormat = await response.ReadResponseBodyAs<ApiReturnDTO<T>>().ConfigureAwait(false);
            return apiReturnFormat.Response;
        }

        public static async Task<GetTokenResponseDTO> GetTokenAsync(LoginDTO loginDto)
        {
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");

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

        public static async Task<Cookie> GetCookieAsync(OrganizationRole role)
        {
            var userCredentials = TestEnvironment.GetCredentials(role);
            var url = TestEnvironment.CreateUrl("api/authorize");
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(userCredentials.Username, userCredentials.Password);

            var cookieResponse = await HttpApi.PostAsync(url, loginDto);
            var cookieParts = cookieResponse.Headers.Where(x => x.Key == "Set-Cookie").First().Value.First().Split('=');
            var cookieName = cookieParts[0];
            var cookieValue = cookieParts[1].Split(';')[0];

            return new Cookie(cookieName, cookieValue)
            {
                Domain = url.Host
            };

        }

        public static async Task<int> CreateOdataUserAsync(ApiUserDTO userDto, string role)
        {
            var cookie = await GetCookieAsync(OrganizationRole.GlobalAdmin);

            var createUserDto = ObjectCreateHelper.MakeSimpleCreateUserDto(userDto);

            int userId;
            using (var createdResponse = await PostWithCookieAsync(TestEnvironment.CreateUrl("odata/Users/Users.Create"), cookie, createUserDto))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAs<UserDTO>().ConfigureAwait(false);
                userId = response.Id;

                Assert.Equal(userDto.Email, response.Email);
            }

            var roleDto = new OrgRightDTO
            {
                UserId = userId,
                Role = role
            };

            using (var addedRole = await PostWithCookieAsync(TestEnvironment.CreateUrl("odata/Organizations(1)/Rights"), cookie, roleDto))
            {
                Assert.Equal(HttpStatusCode.Created, addedRole.StatusCode);
            }

            return userId;
        }

        public static async Task<HttpResponseMessage> PatchOdataUserAsync(ApiUserDTO userDto, int userId)
        {
            var cookie = await GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var patch = await PatchWithCookieAsync(TestEnvironment.CreateUrl($"odata/Users({userId})"), cookie, userDto))
            {
                Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
                return patch;
            };
        }

        public static async Task<HttpResponseMessage> DeleteOdataUserAsync(int id)
        {
            var cookie = await GetCookieAsync(OrganizationRole.GlobalAdmin);
            var response = await DeleteWithCookieAsync(TestEnvironment.CreateUrl("api/user/" + id), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return response;
        }

    }
}
