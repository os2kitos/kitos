using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.DomainModel.Organization;
using System.Threading.Tasks;
using Core.ApplicationServices.Model.Authentication;
using Presentation.Web.Models.API.V2.Request.Token;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Authorization
{
    public class TokenV2Tests : BaseTest
    {
        [Fact]
        public async Task Can_Validate_Token()
        {
            //Arrange
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, DefaultOrgUuid, true, false);

            //Act
            var result = await ValidateToken(token);

            //Assert
            Assert.True(result.Active);
        }
        
        [Fact]
        public async Task Authorization_Fails_If_Token_Is_Invalid()
        {
            //Act
            using var response = await SendValidateTokenRequest(A<string>());

            //Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Only_Global_Admins_Has_CanPublish_Claim(OrganizationRole role)
        {
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), role,
                DefaultOrgUuid, true, false);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var canPublishClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "CanPublish");

            if (role == OrganizationRole.GlobalAdmin)
            {
                Assert.NotNull(canPublishClaim);
                Assert.Equal("true", canPublishClaim.Value);
            }
            else
            {
                Assert.Null(canPublishClaim);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]

        public async Task Only_SystemIntegrators_Has_CanSubscribe_Claim(bool isSystemIntegrator)
        {
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User,
                DefaultOrgUuid, true, false, isSystemIntegrator);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var canSubscribeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "CanSubscribe");
            var hasCanSubscribeClaim = canSubscribeClaim?.Value == "true";
            Assert.Equal(isSystemIntegrator, hasCanSubscribeClaim);
        }

        private static async Task<HttpResponseMessage> SendValidateTokenRequest(string token)
        {
            return await HttpApi.PostAsync(TestEnvironment.CreateUrl("api/v2/token/validate"), new TokenIntrospectionRequest(){ Token = token });
        }
        private static async Task<TokenIntrospectionResponse> ValidateToken(string token)
        {
            using var response = await SendValidateTokenRequest(token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<TokenIntrospectionResponse>();
        }
    }
}
