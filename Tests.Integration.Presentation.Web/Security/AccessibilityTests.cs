using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools.Model;
using Presentation.Web.Models;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser, _globalAdmin;

        public AccessibilityTests()
        {
            _apiUser = TestEnvironment.getApiUser();
            _globalAdmin = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin);
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Forbidden)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK)]
        public async Task LoggedInApiGetRequests(string apiUrl, HttpStatusCode httpCode)
        {
            var loginDto = new LoginDTO
            {
                Email = _apiUser.Username,
                Password = _apiUser.Password
            };

            var token = await HttpApi.GetTokenAsync(loginDto);
            using (var httpResponse = await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl(apiUrl), token.Token))
            {
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Unauthorized)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Unauthorized)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.Unauthorized)]
        public async Task AnonymousApiCalls(string apiUrl, HttpStatusCode httpCode)
        {
            using (var httpResponse = await HttpApi.GetAsync(TestEnvironment.CreateUrl(apiUrl)))
            {
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        } 

        [Theory] 
        [InlineData("/api/Reference", HttpStatusCode.Created)]
        public async Task LoggedInApiPostRequests(string apiUrl, HttpStatusCode httpCode)
        {
            var loginDto = new LoginDTO
            {
                Email = _apiUser.Username,
                Password = _apiUser.Password
            };

            var jobj = new JObject {{"Title", "STRONGMINDS"}, {"ExternalReferenceId", "1338"}, {"URL", "https://strongminds.dk/" },{"Display","0"} };

            var token = await HttpApi.GetTokenAsync(loginDto);
            using (var httpResponse = await HttpApi.PostAsyncWithToken(TestEnvironment.CreateUrl(apiUrl), jobj, token.Token))
            {
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }


        [Fact]
        public async Task Token_Can_Be_Invalidated_After_Creation()
        {
            var email = "test@test.dk";
            var pwd = "arne123";
            var loginDto = new LoginDTO
            {
                Email = email,
                Password = pwd
            };
            var globalRole = _globalAdmin.Role;
            var cookie = await HttpApi.GetCookieAsync(globalRole);

            var userDto = new ApiUserDTO
            {
                Email = email,
                Name = "test",
                LastName = "test2",
                HasApiAccess = true
            };

            var createdUserId = await HttpApi.CreateOdataUser(userDto, "User");
            
            var token = await HttpApi.GetTokenAsync(loginDto);

            using (var requestResponse = await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
            };


            userDto.HasApiAccess = false;

            var patch = await HttpApi.PatchOdataUser(userDto, createdUserId);
            

            using (var requestResponse = await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            };

            var deleteResponse = await HttpApi.DeleteAsyncWithCookie(TestEnvironment.CreateUrl("api/user/" + createdUserId), cookie);

            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

    }
}
