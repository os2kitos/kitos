using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class CSRFProtectionTest : WithAutoFixture
    {

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Modifying_Request_Through_Cookie_Without_CSRF_Token_Is_Rejected(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var itSystem = new
            {
                name = A<string>(),
                belongsToId = TestEnvironment.DefaultOrganizationId,
                organizationId = TestEnvironment.DefaultOrganizationId,
                AccessModifier = AccessModifier.Public
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, TestEnvironment.CreateUrl("api/itsystem"))
            {
                Content = new StringContent(JsonConvert.SerializeObject(itSystem), Encoding.UTF8, "application/json")
            };
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(cookie);

            //Act
            using (var client = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer }))
            {
                var response = await client.SendAsync(requestMessage);
                //Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Modifying_Request_Through_Cookie_With_CSRF_Token_Is_Executed(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var itSystem = new
            {
                name = A<string>(),
                belongsToId = TestEnvironment.DefaultOrganizationId,
                organizationId = TestEnvironment.DefaultOrganizationId,
                AccessModifier = AccessModifier.Public
            };

            //Act
            using (var httpResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("/api/itsystem"), cookie, itSystem))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            }
        }
    }
}
