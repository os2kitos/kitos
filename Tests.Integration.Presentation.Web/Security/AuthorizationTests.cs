using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AuthorizationTests : WithAutoFixture
    {
        private readonly KitosCredentials _localAdmin;

        public AuthorizationTests()
        {
            _localAdmin = TestEnvironment.GetCredentials(OrganizationRole.LocalAdmin);
        }

        [Fact]
        public async Task Can_Get_Token()
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");
            var loginDto = new LoginDTO
            {
                Email = _localAdmin.Username,
                Password = _localAdmin.Password
            };

            //Act
            using (var httpResponseMessage = await HttpApi.PostAsync(url, loginDto))
            {
                //Assert the correct status code and expected content.
                Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
                var tokenResponse = await httpResponseMessage.ReadResponseBodyAsKitosApiResponse<GetTokenResponseDTO>();

                Assert.Equal(loginDto.Email, tokenResponse.Email);
                Assert.True(tokenResponse.LoginSuccessful);
                Assert.True(tokenResponse.Expires > DateTime.UtcNow);
                Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token));
            }
        }

        [Fact]
        public async Task Get_Token_Returns_401_On_Invalid_Password()
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");
            var loginDto = new LoginDTO
            {
                Email = _localAdmin.Username,
                Password = A<string>()
            };

            //Act
            using (var httpResponseMessage = await HttpApi.PostAsync(url, loginDto))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, httpResponseMessage.StatusCode);
            }
        }

        [Fact]
        public async Task Get_Token_Returns_401_On_Invalid_Username()
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");
            var loginDto = new LoginDTO
            {
                Email = A<string>(),
                Password = _localAdmin.Password
            };

            //Act
            using (var httpResponseMessage = await HttpApi.PostAsync(url, loginDto))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, httpResponseMessage.StatusCode);
            }
        }
    }
}
