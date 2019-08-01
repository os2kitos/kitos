using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class ApiAccessibilityTests : WithAutoFixture
    {

        private readonly KitosCredentials _globalAdmin;

        public ApiAccessibilityTests()
        {
            _globalAdmin = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin);
        }

        [Fact]
        public async Task Can_Access_PublicApi_Endpoint()
        {
            var role = _globalAdmin.Role;

            var tokenResponse = await HttpApi.GetTokenAsync(role);
            var requestResponse = await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/ItSystem/"), tokenResponse.Token);
            
            Assert.NotNull(requestResponse);
            Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Not_Access_InternalApi_Endpoint()
        {
            var role = _globalAdmin.Role;

            var tokenResponse = await HttpApi.GetTokenAsync(role);
            var requestResponse = await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/organization/"), tokenResponse.Token);
            var contentAsString = await requestResponse.Content.ReadAsStringAsync();

            Assert.NotNull(requestResponse);
            Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            Assert.Equal("Det er ikke tilladt at benytte dette endpoint", contentAsString);
        }

        [Fact]
        public async Task Can_Not_Access_Odata_Endpoint()
        {
            var role = _globalAdmin.Role;

            var tokenResponse = await HttpApi.GetTokenAsync(role);
            var requestResponse = await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("odata/Organizations(1)/"), tokenResponse.Token);
            var contentAsString = await requestResponse.Content.ReadAsStringAsync();

            Assert.NotNull(requestResponse);
            Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            Assert.Equal("Det er ikke tilladt at kalde odata endpoints", contentAsString);
        }

    }
}
