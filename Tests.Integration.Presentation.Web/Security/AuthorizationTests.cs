using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AuthorizationTests
    {
        [Fact]
        public async Task Can_Get_Token()
        {
            //Arrange
            var credentials = TestEnvironment.GetCredentials(OrganizationRole.LocalAdmin);
            var url = TestEnvironment.CreateUrl("api/authorize/GetToken");
            var loginDto = new LoginDTO
            {
                Email = credentials.Username,
                Password = credentials.Password
            };

            //Act
            using (var httpResponseMessage = await HttpApi.PostAsync(url, loginDto))
            {
                //Assert the correct status code and expected content.
                Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
                var tokenResponse = await httpResponseMessage.ReadResponseBodyAsKitosApiResponse<GetTokenResponseDTO>();

                Assert.Equal(loginDto.Email, tokenResponse.Email);
                Assert.True(tokenResponse.LoginSuccessful);
                Assert.True(tokenResponse.Expires > DateTime.UtcNow);
                Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token));
            }
        }
    }
}
