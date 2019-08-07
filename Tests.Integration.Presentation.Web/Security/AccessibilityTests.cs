using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools.Model;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser;

        public AccessibilityTests()
        {
            _apiUser = TestEnvironment.GetCredentials(OrganizationRole.ApiAccess);
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Forbidden)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK)]
        public async Task LoggedInApiGetRequests(string apiUrl, HttpStatusCode httpCode)
        {
            var role = _apiUser.Role;
            var token = await HttpApi.GetTokenAsync(role);
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
            var role = _apiUser.Role;

            var jobj = new JObject {{"Title", "STRONGMINDS"}, {"ExternalReferenceId", "1338"}, {"URL", "https://strongminds.dk/" },{"Display","0"} };

            var token = await HttpApi.GetTokenAsync(role);
            var test = 0;
            using (var httpResponse = await HttpApi.PostAsyncWithToken(TestEnvironment.CreateUrl(apiUrl), jobj, token.Token))
            {
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }

    }
}
