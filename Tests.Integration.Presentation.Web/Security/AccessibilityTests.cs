using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.Users;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests : BaseTest
    {
        private readonly string _defaultPassword;

        public AccessibilityTests()
        {
            _defaultPassword = TestEnvironment.GetDefaultUserPassword();
        }

        [Theory]
        [InlineData("api/v2/organizations", HttpStatusCode.OK)]
        public async Task Api_Get_Requests_Using_Token(string apiUrl, HttpStatusCode httpCode)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(apiUrl), token.Token);

            //Assert
            Assert.Equal(httpCode, httpResponse.StatusCode);
        }

        [Theory]
        [InlineData("api/v2/organizations", HttpStatusCode.Unauthorized)]
        public async Task Anonymous_Api_Calls_Returns_401(string apiUrl, HttpStatusCode httpCode)
        {
            using var httpResponse = await HttpApi.GetAsync(TestEnvironment.CreateUrl(apiUrl));

            Assert.Equal(httpCode, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Token_Can_Be_Invalidated_After_Creation()
        {
            //Arrange
            var email = CreateEmail();
            var createdUser = await CreateUserAsync(DefaultOrgUuid, email, true);
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(email, _defaultPassword);
            var token = await HttpApi.GetTokenAsync(loginDto);
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/v2/organizations"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
            }

            //Act
            await DisableApiAccessForUserAsync(DefaultOrgUuid, createdUser.Uuid);

            //Assert
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/v2/organizations"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            }
            await UsersV2Helper.DeleteUserGlobally(createdUser.Uuid);
        }

        private static async Task DisableApiAccessForUserAsync(Guid organizationUuid, Guid userUuid)
        {
            await UsersV2Helper.PatchUserAsync(organizationUuid, userUuid, x => x.HasApiAccess, false);
        }

    }
}
