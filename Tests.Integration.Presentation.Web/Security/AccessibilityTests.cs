using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Tests.Integration.Presentation.Web.Tools.Model;
using Tests.Toolkit.Patterns;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests : WithAutoFixture
    {
        private readonly string _defaultPassword;

        public AccessibilityTests()
        {
            _defaultPassword = TestEnvironment.GetDefaultUserPassword();
        }

        [Theory]
        [InlineData("api/v2/organizations", HttpStatusCode.Forbidden)]
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
            var userDto = ObjectCreateHelper.MakeSimpleApiUserDto(email, true);
            var createdUserId = await HttpApi.CreateOdataUserAsync(userDto, OrganizationRole.User);
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(email, _defaultPassword);
            var token = await HttpApi.GetTokenAsync(loginDto);
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/v2/organizations"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
            };

            //Act
            await DisableApiAccessForUserAsync(userDto, createdUserId);

            //Assert
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/v2/organizations"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            };
            await UserHelper.SendDeleteUserAsync(createdUserId).DisposeAsync();
        }

        private static string CreateEmail()
        {
            return $"{Guid.NewGuid():N}@test.dk";
        }

        private static async Task DisableApiAccessForUserAsync(ApiUserDTO userDto, int id)
        {
            userDto.HasApiAccess = false;
            await HttpApi.PatchOdataUserAsync(userDto, id);
        }

    }
}
