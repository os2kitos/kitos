using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AuthorizationTests : WithAutoFixture
    {
        private readonly KitosCredentials _regularApiUser, _globalAdmin;
        private readonly Uri _getTokenUrl;

        public AuthorizationTests()
        {
            _regularApiUser = TestEnvironment.GetCredentials(OrganizationRole.User, true);
            _globalAdmin = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin);
            _getTokenUrl = TestEnvironment.CreateUrl("api/authorize/GetToken");
        }

        [Fact]
        public async Task Api_Access_User_Can_Get_Token()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_regularApiUser.Username, _regularApiUser.Password);

            //Act
            var tokenResponse = await HttpApi.GetTokenAsync(loginDto);

            //Assert
            Assert.NotNull(tokenResponse);
            Assert.True(tokenResponse.LoginSuccessful);
            Assert.True(tokenResponse.Expires > DateTime.UtcNow);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token));
        }

        [Fact]
        public async Task User_Without_Api_Access_Can_Not_Get_Token()
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_globalAdmin.Username, _globalAdmin.Password);

            //Act
            var tokenResponse = await HttpApi.PostAsync(url, loginDto);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, tokenResponse.StatusCode);
        }

        [Fact]
        public async Task Get_Token_Returns_401_On_Invalid_Password()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_regularApiUser.Username, A<string>());
            
            //Act
            using (var httpResponseMessage = await HttpApi.PostAsync(_getTokenUrl, loginDto))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, httpResponseMessage.StatusCode);
            }
        }

        [Fact]
        public async Task Get_Token_Returns_401_On_Invalid_Username()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(A<string>(), _regularApiUser.Password);

            //Act
            using (var httpResponseMessage = await HttpApi.PostAsync(_getTokenUrl, loginDto))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, httpResponseMessage.StatusCode);
            }
        }

    }
}
