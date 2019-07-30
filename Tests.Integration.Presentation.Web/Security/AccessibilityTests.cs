using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests
    {
        public AccessibilityTests()
        {

        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.OK, OrganizationRole.User)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden, OrganizationRole.User)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK, OrganizationRole.User)]

        [InlineData("api/User", HttpStatusCode.OK, OrganizationRole.LocalAdmin)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden, OrganizationRole.LocalAdmin)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK, OrganizationRole.LocalAdmin)]
        
        [InlineData("api/User", HttpStatusCode.OK, OrganizationRole.GlobalAdmin)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.OK, OrganizationRole.GlobalAdmin)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK, OrganizationRole.GlobalAdmin)]
        public async Task LoggedInApiCalls(string apiUrl, HttpStatusCode httpCode, OrganizationRole role)
        {
            var token = await HttpApi.GetTokenAsync(role);
            var httpResponse = await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl(apiUrl), token.Token);
            Assert.Equal(httpCode, httpResponse.StatusCode);
        }

        [Theory]
        [InlineData("api/User", HttpStatusCode.Unauthorized)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Unauthorized)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.Unauthorized)]
        public async Task AnonymousApiCalls(string apiUrl, HttpStatusCode httpCode)
        {
            var httpResponse = await HttpApi.GetAsync(TestEnvironment.CreateUrl(apiUrl));
            Assert.Equal(httpCode, httpResponse.StatusCode);
        }


    }
}
