using System;
using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools.Model;
using Presentation.Web.Models;
using System.Net.Http;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser, _globalAdmin;
        private readonly string _defaultPassword;

        public AccessibilityTests()
        {
            _apiUser = TestEnvironment.GetApiUser();
            _globalAdmin = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin);
            _defaultPassword = TestEnvironment.GetDefaultUserPassword();
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Forbidden)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK)]
        public async Task Api_Get_Requests_Using_Token(string apiUrl, HttpStatusCode httpCode)
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_apiUser.Username, _apiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(apiUrl), token.Token))
            {
                //Assert
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Unauthorized)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Unauthorized)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.Unauthorized)]
        public async Task Anonymous_Api_Calls_Returns_401(string apiUrl, HttpStatusCode httpCode)
        {
            using (var httpResponse = await HttpApi.GetAsync(TestEnvironment.CreateUrl(apiUrl)))
            {
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Post_Reference_With_Valid_Input_Returns_201()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_apiUser.Username, _apiUser.Password);
            var payload = new
            {
                Title = A<string>(),
                ExternalReferenceId = A<string>(),
                URL = "https://strongminds.dk/"
            };
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl("/api/Reference"), payload, token.Token))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            }
        }


        [Fact]
        public async Task Token_Can_Be_Invalidated_After_Creation()
        {
            //Arrange
            var email = CreateEmail();
            var userDto = ObjectCreateHelper.MakeSimpleApiUserDto(email, true);
            var createdUserId = await HttpApi.CreateOdataUserAsync(userDto, "User");
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(email, _defaultPassword);
            var token = await HttpApi.GetTokenAsync(loginDto);
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
            };
            
            //Act
            await DisableApiAccessForUserAsync(userDto, createdUserId);

            //Assert
            using (var requestResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            };
            var deleteResponse = await HttpApi.DeleteOdataUserAsync(createdUserId);
        }

        private static string CreateEmail()
        {
            return $"{Guid.NewGuid():N}@test.dk";
        }

        private async Task DisableApiAccessForUserAsync(ApiUserDTO userDto, int id)
        {
            userDto.HasApiAccess = false;
            await HttpApi.PatchOdataUserAsync(userDto, id);
        }
    
    }
}
