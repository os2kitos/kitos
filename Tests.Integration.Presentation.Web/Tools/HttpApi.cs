using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Cryptography;
using Newtonsoft.Json;
using Polly;
using Presentation.Web.Helpers;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools.Model;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class HttpApi
    {
        public class StatefulScope : IDisposable
        {
            private static readonly object QueueLock = new();
            private static readonly Queue<(HttpClient client, HttpClientHandler handler, CookieContainer cookieContainer)> StatefulHttpClients;

            static StatefulScope()
            {
                ConfigureServicePointManager();
                StatefulHttpClients = Enumerable
                    .Range(0, Environment.ProcessorCount * 2).Select(_ => CreateClient())
                    .Transform(clients => new Queue<(HttpClient client, HttpClientHandler handler, CookieContainer cookieContainer)>(clients));
            }

            private static (HttpClient httpClient, HttpClientHandler httpClientHandler, CookieContainer cookieContainer) CreateClient()
            {
                var cookieContainer = new CookieContainer();
                var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer };
                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.ConnectionClose = true;
                httpClientHandler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true;
                return (httpClient, httpClientHandler, cookieContainer);
            }

            public static StatefulScope Create()
            {
                (HttpClient client, HttpClientHandler handler, CookieContainer cookieContainer) result;
                lock (QueueLock)
                {
                    //If no available client, grow the pool by one
                    result = StatefulHttpClients.Any() ? StatefulHttpClients.Dequeue() : CreateClient();
                }

                foreach (Cookie cookie in result.cookieContainer.GetCookies(new Uri(TestEnvironment.GetBaseUrl())))
                {
                    cookie.Expires = DateTime.UtcNow.AddYears(-1);
                }

                return new StatefulScope(result.client, result.handler, result.cookieContainer);
            }

            private bool isDisposed;

            public HttpClient Client { get; }
            public HttpClientHandler ClientHandler { get; }
            public CookieContainer CookieContainer { get; }

            public StatefulScope(HttpClient client, HttpClientHandler clientHandler, CookieContainer cookieContainer)
            {
                Client = client;
                ClientHandler = clientHandler;
                CookieContainer = cookieContainer;
            }

            public void Dispose()
            {
                lock (QueueLock)
                {
                    if (!isDisposed)
                    {
                        StatefulHttpClients.Enqueue((Client, ClientHandler, CookieContainer));
                        isDisposed = true;
                    }
                }
            }
        }
        private static IEnumerable<TimeSpan> CreateDurations(params int[] durationInSeconds) => durationInSeconds.Select(s => TimeSpan.FromMilliseconds(s)).ToArray();

        private static readonly IEnumerable<TimeSpan> BackOffDurations = CreateDurations(100, 500, 2000).ToList().AsReadOnly();
        private static readonly ConcurrentDictionary<string, Cookie> CookiesCache = new();
        private static readonly ConcurrentDictionary<string, GetTokenResponseDTO> TokenCache = new();

        /// <summary>
        /// Use for stateless calls only
        /// </summary>
        private static readonly HttpClient StatelessHttpClient;

        static HttpApi()
        {
            ConfigureServicePointManager();
            StatelessHttpClient = new(new HttpClientHandler { UseCookies = false, ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true });
            StatelessHttpClient.DefaultRequestHeaders.ExpectContinue = false;
            StatelessHttpClient.DefaultRequestHeaders.ConnectionClose = true;
        }

        public static void ConfigureServicePointManager()
        {
            ServicePointManager.SecurityProtocol = EnumRange
                .All<SecurityProtocolType>()
                .Where(protocol => protocol >= SecurityProtocolType.Tls12)
                .Aggregate(SecurityProtocolType.SystemDefault, (acc, next) => acc | next);

            ServicePointManager.Expect100Continue = false;
        }

        public static Task<HttpResponseMessage> GetWithTokenAsync(Uri url, string token, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            if (headers != null)
            {
                foreach (var nameAndValue in headers)
                {
                    requestMessage.Headers.Add(nameAndValue.Key, nameAndValue.Value);
                }
            }
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            return StatelessHttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> PostWithTokenAsync(Uri url, object body, string token)
        {
            var requestMessage = CreatePostMessage(url, body);
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            return StatelessHttpClient.SendAsync(requestMessage);
        }
        public static Task<HttpResponseMessage> PutWithTokenAsync(Uri url, string token, object body = null)
        {
            var requestMessage = CreatePutMessage(url, body);
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            return StatelessHttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> PatchWithTokenAsync(Uri url, string token, object body = null)
        {
            var requestMessage = CreatePatchMessage(url, body);
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/merge-patch+json");
            return StatelessHttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> DeleteWithTokenAsync(Uri url, string token, object body = null)
        {
            var requestMessage = CreateMessageWithContent(HttpMethod.Delete, url, body);
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse("bearer " + token);
            return StatelessHttpClient.SendAsync(requestMessage);
        }

        public static Task<HttpResponseMessage> PostWithCookieAsync(Uri url, Cookie cookie, object body, bool acceptUnAuthorized = false)
        {
            return WithRetryPolicy(async () =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = body as HttpContent ?? new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };

                return await SendWithCookieAsync(cookie, requestMessage);
            }, acceptUnAuthorized == false);
        }

        public static Task<HttpResponseMessage> PutWithCookieAsync(Uri url, Cookie cookie, object body = null, bool acceptUnAuthorized = false)
        {
            return WithRetryPolicy(async () =>
            {
                var requestMessage = CreatePutMessage(url, body);

                return await SendWithCookieAsync(cookie, requestMessage);
            }, acceptUnAuthorized == false);
        }

        public static Task<HttpResponseMessage> GetWithCookieAsync(Uri url, Cookie cookie, bool acceptUnAuthorized = false)
        {
            return WithRetryPolicy(async () =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                return await SendWithCookieAsync(cookie, requestMessage);
            }, acceptUnAuthorized == false);
        }

        public static Task<HttpResponseMessage> DeleteWithCookieAsync(Uri url, Cookie cookie, object body = null, bool acceptUnAuthorized = false)
        {
            return WithRetryPolicy(async () =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
                if (body != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                }

                return await SendWithCookieAsync(cookie, requestMessage);
            }, acceptUnAuthorized == false);
        }

        public static Task<HttpResponseMessage> PatchWithCookieAsync(Uri url, Cookie cookie, object body, bool acceptUnAuthorized = false)
        {
            return WithRetryPolicy(async () =>
            {
                var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };

                return await SendWithCookieAsync(cookie, requestMessage);
            }, acceptUnAuthorized == false);
        }

        private static async Task<HttpResponseMessage> SendWithCookieAsync(Cookie cookie, HttpRequestMessage requestMessage)
        {
            return await SendWithCSRFToken(requestMessage, cookie);
        }

        private static async Task<HttpResponseMessage> SendWithCSRFToken(HttpRequestMessage requestMessage,
            Cookie authCookie = null)
        {
            var csrfToken = await GetCSRFToken(authCookie);
            requestMessage.Headers.Add(Constants.CSRFValues.HeaderName, csrfToken.FormToken);

            using (var scope = StatefulScope.Create())
            {
                if (authCookie != null)
                {
                    scope.CookieContainer.Add(authCookie);
                }
                scope.CookieContainer.Add(csrfToken.CookieToken);
                return await scope.Client.SendAsync(requestMessage);
            }
        }

        public static async Task<CSRFTokenDTO> GetCSRFToken(Cookie authCookie = null)
        {
            var url = TestEnvironment.CreateUrl("api/authorize/antiforgery");
            var csrfRequest = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage csrfResponse = null;
            try
            {
                if (authCookie == null)
                {
                    using (var scope = StatefulScope.Create())
                    {
                        csrfResponse = await scope.Client.SendAsync(csrfRequest);
                    }
                }
                else
                {
                    using (var scope = StatefulScope.Create())
                    {
                        scope.CookieContainer.Add(authCookie);
                        csrfResponse = await scope.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
                    }
                }

                Assert.Equal(HttpStatusCode.OK, csrfResponse.StatusCode);
                var cookieParts = csrfResponse.Headers.First(x => x.Key == "Set-Cookie").Value.First().Split('=');
                var cookie = new Cookie(Constants.CSRFValues.CookieName, cookieParts[1].Split(';')[0], "/", url.Host);
                return new CSRFTokenDTO
                {
                    CookieToken = cookie,
                    FormToken = await csrfResponse.ReadResponseBodyAsAsync<string>()
                };
            }
            finally
            {
                csrfResponse?.Dispose();
            }
        }

        public static Task<HttpResponseMessage> PostAsync(Uri url, object body)
        {
            var requestMessage = CreatePostMessage(url, body);
            return SendWithCSRFToken(requestMessage);
        }

        private static HttpRequestMessage CreatePostMessage(Uri url, object body)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            return requestMessage;
        }
        private static HttpRequestMessage CreatePutMessage(Uri url, object body)
        {
            return CreateMessageWithContent(HttpMethod.Put, url, body);
        }

        private static HttpRequestMessage CreatePatchMessage(Uri url, object body)
        {
            return CreateMessageWithContent(new HttpMethod("PATCH"), url, body);
        }

        private static HttpRequestMessage CreateMessageWithContent(HttpMethod method, Uri url, object body)
        {
            var requestMessage = new HttpRequestMessage(method, url)
            {
                Content = body?.Transform(content =>
                    new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"))
            };
            return requestMessage;
        }

        public static Task<HttpResponseMessage> GetAsync(Uri url)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            return StatelessHttpClient.SendAsync(requestMessage);
        }

        public static async Task<T> ReadResponseBodyAsAsync<T>(this HttpResponseMessage response)
        {
            var responseAsJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseAsJson);
        }

        public static async Task<List<T>> ReadOdataListResponseBodyAsAsync<T>(this HttpResponseMessage response)
        {
            var responseAsJson = await response.Content.ReadAsStringAsync();
            var spec = new { value = new List<T>() };
            var result = JsonConvert.DeserializeAnonymousType(responseAsJson, spec);
            return result.value;
        }

        public static async Task<T> ReadResponseBodyAsKitosApiResponseAsync<T>(this HttpResponseMessage response)
        {
            var apiReturnFormat = await response.ReadResponseBodyAsAsync<ApiReturnDTO<T>>();
            return apiReturnFormat.Response;
        }

        private static async Task<HttpResponseMessage> WithRetryPolicy(Func<Task<HttpResponseMessage>> callApi, bool retryOnUnAuthorized = true)
        {
            if (retryOnUnAuthorized)
            {
                return await Policy
                    .Handle<Exception>(e => true) //outer policy handles transient protocol errors, connection timeouts, task cancellations and so on
                    .WaitAndRetryAsync(BackOffDurations)
                    .ExecuteAsync(() =>
                    {
                        return Policy
                            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized) //Inner policy deals with response related errors
                            .WaitAndRetryAsync(BackOffDurations, (result, _, _, _) => result.Result.Dispose())
                            .ExecuteAsync(callApi);
                    });
            }

            return await callApi();
        }

        public static async Task<HttpResponseMessage> PostForKitosToken(Uri url, LoginDTO loginDto)
        {
            return await WithRetryPolicy(() => StatelessHttpClient.SendAsync(CreatePostMessage(url, loginDto)));
        }

        public static async Task<GetTokenResponseDTO> GetTokenAsync(OrganizationRole role)
        {
            var userCredentials = TestEnvironment.GetCredentials(role, true);

            return await GetTokenAsync(userCredentials);
        }

        private static async Task<GetTokenResponseDTO> GetTokenAsync(KitosCredentials userCredentials)
        {
            if (TokenCache.TryGetValue(userCredentials.Username, out var cachedToken))
                return cachedToken;

            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");

            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(userCredentials.Username, userCredentials.Password);

            using var httpResponseMessage = await PostForKitosToken(url, loginDto);
            var tokenResponseDtoAsync = await GetTokenResponseDtoAsync(loginDto, httpResponseMessage);
            TokenCache.TryAdd(userCredentials.Username, tokenResponseDtoAsync);
            return tokenResponseDtoAsync;
        }

        public static async Task<GetTokenResponseDTO> GetTokenAsync(LoginDTO loginDto)
        {
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");

            using var httpResponseMessage = await PostForKitosToken(url, loginDto);
            return await GetTokenResponseDtoAsync(loginDto, httpResponseMessage);
        }

        private static async Task<GetTokenResponseDTO> GetTokenResponseDtoAsync(LoginDTO loginDto, HttpResponseMessage httpResponseMessage)
        {
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            var tokenResponse = await httpResponseMessage.ReadResponseBodyAsKitosApiResponseAsync<GetTokenResponseDTO>();

            Assert.Equal(loginDto.Email, tokenResponse.Email);
            Assert.True(tokenResponse.LoginSuccessful);
            Assert.True(tokenResponse.Expires > DateTime.UtcNow);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token));

            return tokenResponse;
        }

        public static async Task<Cookie> GetCookieAsync(KitosCredentials userCredentials, bool acceptUnAuthorized = false)
        {
            if (CookiesCache.TryGetValue(userCredentials.Username, out var cachedCookie))
                return cachedCookie;

            var cookieResponse = await SendGetCookieAsync(userCredentials);

            Assert.Equal(HttpStatusCode.Created, cookieResponse.StatusCode);
            var cookieParts = cookieResponse.Headers.First(x => x.Key == "Set-Cookie").Value.First().Split('=');
            var cookieName = cookieParts[0];
            var cookieValue = cookieParts[1].Split(';')[0];

            var cookie = new Cookie(cookieName, cookieValue)
            {
                Domain = cookieResponse.RequestMessage.RequestUri.Host
            };
            CookiesCache.TryAdd(userCredentials.Username, cookie);
            return cookie;
        }

        public static async Task<HttpResponseMessage> SendGetCookieAsync(KitosCredentials userCredentials)
        {
            var url = TestEnvironment.CreateUrl("api/authorize");
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(userCredentials.Username, userCredentials.Password);

            return await WithRetryPolicy(async () =>
            {
                var request = CreatePostMessage(url, loginDto);

                return await SendWithCSRFToken(request);
            });
        }

        public static async Task<Cookie> GetCookieAsync(OrganizationRole role, bool acceptUnAuthorized = false)
        {
            var userCredentials = TestEnvironment.GetCredentials(role);
            return await GetCookieAsync(userCredentials, acceptUnAuthorized);
        }

        public static async Task<(int userId, KitosCredentials credentials, Cookie loginCookie)> CreateUserAndLogin(string email, OrganizationRole role, int organizationId = TestEnvironment.DefaultOrganizationId, bool apiAccess = false, bool hasStakeHolderAccess = false)
        {
            var userId = await CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(email, apiAccess, hasStakeHolderAccess), role, organizationId);
            var password = Guid.NewGuid().ToString("N");
            DatabaseAccess.MutateEntitySet<User>(x =>
            {
                using var crypto = new CryptoService();
                var user = x.AsQueryable().ById(userId);
                user.Password = crypto.Encrypt(password + user.Salt);
            });

            var cookie = await GetCookieAsync(new KitosCredentials(email, password));

            return (userId, new KitosCredentials(email, password), cookie);
        }

        public static async Task<(int userId, KitosCredentials credentials, Cookie loginCookie)> CreateUserAndLogin(string email, OrganizationRole role, string name, string lastName, int organizationId = TestEnvironment.DefaultOrganizationId, bool apiAccess = false)
        {
            var userInfo = await CreateUser(email, role, name, lastName, organizationId, apiAccess);

            var cookie = await GetCookieAsync(new KitosCredentials(email, userInfo.password));

            return (userInfo.userId, new KitosCredentials(email, userInfo.password), cookie);
        }

        private static async Task<(int userId, string password)> CreateUser(
            string email,
            OrganizationRole role,
            string name,
            string lastName,
            int organizationId,
            bool apiAccess)
        {
            var userId = await CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(email, apiAccess), role, organizationId);
            var password = Guid.NewGuid().ToString("N");
            DatabaseAccess.MutateEntitySet<User>(x =>
            {
                using var crypto = new CryptoService();
                var user = x.AsQueryable().ById(userId);
                user.Password = crypto.Encrypt(password + user.Salt);
                user.Name = name;
                user.LastName = lastName;
            });
            return (userId, password);
        }

        public static async Task<(int userId, KitosCredentials credentials, string token)> CreateUserAndGetToken(string email, OrganizationRole role, int organizationId = TestEnvironment.DefaultOrganizationId, bool apiAccess = false, bool stakeHolderAccess = false)
        {
            var userId = await CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(email, apiAccess, stakeHolderAccess), role, organizationId);
            var password = Guid.NewGuid().ToString("N");
            DatabaseAccess.MutateEntitySet<User>(x =>
            {
                using var crypto = new CryptoService();
                var user = x.AsQueryable().ById(userId);
                user.Password = crypto.Encrypt(password + user.Salt);
                user.IsGlobalAdmin = role == OrganizationRole.GlobalAdmin;
            });

            var token = await GetTokenAsync(new KitosCredentials(email, password));

            return (userId, new KitosCredentials(email, password), token.Token);
        }

        public static async Task<int> CreateOdataUserAsync(ApiUserDTO userDto, OrganizationRole role, int organizationId = TestEnvironment.DefaultOrganizationId)
        {
            var cookie = await GetCookieAsync(OrganizationRole.GlobalAdmin);

            var createUserDto = ObjectCreateHelper.MakeSimpleCreateUserDto(userDto);

            int userId;
            using (var createdResponse = await PostWithCookieAsync(TestEnvironment.CreateUrl("odata/Users/Users.Create"), cookie, createUserDto))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsAsync<UserDTO>();
                userId = response.Id;

                Assert.Equal(userDto.Email, response.Email);
            }

            using (var addedRole = await SendAssignRoleToUserAsync(userId, role, organizationId))
            {
                Assert.Equal(HttpStatusCode.Created, addedRole.StatusCode);
            }

            return userId;
        }

        public static async Task<HttpResponseMessage> SendAssignRoleToUserAsync(int userId, OrganizationRole role, int organizationId = TestEnvironment.DefaultOrganizationId, Cookie optionalLoginCookie = null)
        {
            var cookie = optionalLoginCookie ?? await GetCookieAsync(OrganizationRole.GlobalAdmin);

            var roleDto = new OrgRightDTO
            {
                UserId = userId,
                Role = role.ToString("G")
            };

            return await PostWithCookieAsync(TestEnvironment.CreateUrl($"odata/Organizations({organizationId})/Rights"), cookie, roleDto);
        }

        public static async Task<HttpResponseMessage> SendRemoveRoleToUserAsync(int rightId, int organizationId = TestEnvironment.DefaultOrganizationId, Cookie optionalLoginCookie = null)
        {
            var cookie = optionalLoginCookie ?? await GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await DeleteWithCookieAsync(TestEnvironment.CreateUrl($"odata/Organizations({organizationId})/Rights({rightId})"), cookie);
        }

        public static async Task PatchOdataUserAsync(ApiUserDTO userDto, int userId)
        {
            var cookie = await GetCookieAsync(OrganizationRole.GlobalAdmin);

            using var patch = await PatchWithCookieAsync(TestEnvironment.CreateUrl($"odata/Users({userId})"), cookie, userDto);
            Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
        }

        public static readonly string OdataDateTimeFormat = "O"; //ISO 8601
    }
}
