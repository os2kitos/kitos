using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AuthorizationTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser, _globalAdmin;
        private readonly Uri _getTokenUrl;

        public AuthorizationTests()
        {
            _apiUser = TestEnvironment.GetCredentials(OrganizationRole.ApiAccess);
            _globalAdmin = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin);
            _getTokenUrl = TestEnvironment.CreateUrl("api/authorize/GetToken");
        }

        [Fact]
        public async Task Api_Access_User_Can_Get_Token()
        {
            var role = _apiUser.Role;

            var tokenResponse = await HttpApi.GetTokenAsync(role);

            Assert.NotNull(tokenResponse);
            Assert.True(tokenResponse.LoginSuccessful);
            Assert.True(tokenResponse.Expires > DateTime.UtcNow);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token));
        }

        [Fact]
        public async Task Non_Api_Access_User_Can_Not_Get_Token()
        {
            var role = _globalAdmin.Role;
            
            var tokenResponse = await HttpApi.NoApiGetTokenAsync(role);
            
            Assert.Equal(HttpStatusCode.Forbidden, tokenResponse.StatusCode);
        }

        [Fact]
        public async Task Get_Token_Returns_401_On_Invalid_Password()
        {
            var loginDto = new LoginDTO
            {
                Email = _apiUser.Username,
                Password = A<string>()
            };

            using (var httpResponseMessage = await HttpApi.PostAsync(_getTokenUrl, loginDto))
            {
                Assert.Equal(HttpStatusCode.Unauthorized, httpResponseMessage.StatusCode);
            }
        }

        [Fact]
        public async Task Get_Token_Returns_401_On_Invalid_Username()
        {
            var loginDto = new LoginDTO
            {
                Email = A<string>(),
                Password = _apiUser.Password
            };

            using (var httpResponseMessage = await HttpApi.PostAsync(_getTokenUrl, loginDto))
            {
                Assert.Equal(HttpStatusCode.Unauthorized, httpResponseMessage.StatusCode);
            }
        }

        [Fact]
        public async Task Token_Can_Be_Invalidated_After_Creation()
        {
            var globalRole = _globalAdmin.Role;
            var cookie = await HttpApi.GetCookieAsync(globalRole);

            var apiRole = _apiUser.Role;
            var token = await HttpApi.GetTokenAsync(apiRole);

            using (var requestResponse =
                await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
            };

            var rights = await HttpApi.GetAsync(TestEnvironment.CreateUrl("odata/Organizations(1)/Rights"));
            var rightsAsString = rights.Content.ReadAsStringAsync();
            var json = JObject.Parse(rightsAsString.Result);
            var idOfRightToDelete = default(int);
            foreach (var rightsElements in json.Last.First)
            {
                if (rightsElements.Value<string>("Role") == "ApiAccess")
                {
                    idOfRightToDelete = rightsElements.Value<int>("Id");
                }
            }
            var delete = await HttpApi.DeleteAsyncWithCookie(TestEnvironment.CreateUrl($"odata/Organizations(1)/Rights({idOfRightToDelete})"), cookie);
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);


            using (var requestResponse =
                await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            };

            var orgRightDTO = new OrgRightDTO()
            {
                Role = OrganizationRole.ApiAccess.ToString(),
                UserId = 5.ToString(),
                OrganizationId = 1.ToString()
            };

            var resp = await HttpApi.PostAsyncWithCookie(TestEnvironment.CreateUrl("odata/Organizations(1)/Rights"), cookie,
                orgRightDTO);

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        }

    }
}
