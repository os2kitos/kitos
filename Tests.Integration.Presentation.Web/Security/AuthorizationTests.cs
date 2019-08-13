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
            _apiUser = TestEnvironment.GetApiUser();
            _globalAdmin = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin);
            _getTokenUrl = TestEnvironment.CreateUrl("api/authorize/GetToken");
        }

        [Fact]
        public async Task Api_Access_User_Can_Get_Token()
        {
            var loginDto = new LoginDTO
            {
                Email = _apiUser.Username,
                Password = _apiUser.Password
            };

            var tokenResponse = await HttpApi.GetTokenAsync(loginDto);

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

    }
}
