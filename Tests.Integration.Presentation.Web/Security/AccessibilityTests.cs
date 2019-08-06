using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Presentation.Web.Helpers;
using Xunit;
using Core.DomainModel.Organization;

namespace Tests.Integration.Presentation.Web.Security
{
    public class AccessibilityTests
    {
        [Theory]
        [InlineData("api/User", HttpStatusCode.Forbidden, OrganizationRole.User)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden, OrganizationRole.User)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK, OrganizationRole.User)]

        [InlineData("api/User", HttpStatusCode.Forbidden, OrganizationRole.LocalAdmin)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden, OrganizationRole.LocalAdmin)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK, OrganizationRole.LocalAdmin)]
        
        [InlineData("api/User", HttpStatusCode.Forbidden, OrganizationRole.GlobalAdmin)]
        [InlineData("api/GlobalAdmin", HttpStatusCode.Forbidden, OrganizationRole.GlobalAdmin)]
        [InlineData("api/ItSystem/?csv&orgUnitId=1&onlyStarred=true&orgUnitId=1r", HttpStatusCode.OK, OrganizationRole.GlobalAdmin)]
        public async Task LoggedInApiGetRequests(string apiUrl, HttpStatusCode httpCode, OrganizationRole role)
        {
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
        [InlineData("/api/Reference", HttpStatusCode.Created, OrganizationRole.GlobalAdmin)]
        public async Task LoggedInApiPostRequests(string apiUrl, HttpStatusCode httpCode, OrganizationRole role)
        {
            var jobj = new JObject {{"Title", "STRONGMINDS"}, {"ExternalReferenceId", "1338"}, {"URL", "https://strongminds.dk/" },{"Display","0"} };

            var token = await HttpApi.GetTokenAsync(role);
            using (var httpResponse = await HttpApi.PostAsyncWithToken(TestEnvironment.CreateUrl(apiUrl), jobj, token.Token))
            {
                Assert.Equal(httpCode, httpResponse.StatusCode);
            }
        }

    }
}
