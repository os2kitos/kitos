using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Helpers;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    public class CSRFProtectionTest : BaseTest
    {
        private const string ItSystemUrl = "api/v2/it-systems";

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

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, TestEnvironment.CreateUrl(ItSystemUrl))
            {
                Content = new StringContent(JsonConvert.SerializeObject(itSystem), Encoding.UTF8, "application/json")
            };

            //Act
            using (var scope = HttpApi.StatefulScope.Create())
            {
                scope.CookieContainer.Add(cookie);

                using var response = await scope.Client.SendAsync(requestMessage);

                //Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Modifying_Request_Through_Cookie_Without_CSRF_Cookie_Is_Rejected(OrganizationRole role)
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

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, TestEnvironment.CreateUrl(ItSystemUrl))
            {
                Content = new StringContent(JsonConvert.SerializeObject(itSystem), Encoding.UTF8, "application/json")
            };

            var csrfToken = await HttpApi.GetCSRFToken(cookie);
            requestMessage.Headers.Add(Constants.CSRFValues.HeaderName, csrfToken.FormToken);

            //Act
            using (var scope = HttpApi.StatefulScope.Create())
            {
                scope.CookieContainer.Add(cookie);
                using var response = await scope.Client.SendAsync(requestMessage);

                //Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Modifying_Request_Through_Cookie_Without_CSRF_Header_Is_Rejected(OrganizationRole role)
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

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, TestEnvironment.CreateUrl(ItSystemUrl))
            {
                Content = new StringContent(JsonConvert.SerializeObject(itSystem), Encoding.UTF8, "application/json")
            };

            var csrfToken = await HttpApi.GetCSRFToken(cookie);

            //Act
            using (var scope = HttpApi.StatefulScope.Create())
            {
                scope.CookieContainer.Add(cookie);
                scope.CookieContainer.Add(csrfToken.CookieToken);
                using var response = await scope.Client.SendAsync(requestMessage);

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
            var request = new CreateItSystemRequestDTO
            {
                OrganizationUuid = DefaultOrgUuid,
                Name = A<string>()
            };

            //Act
            using (var httpResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl(ItSystemUrl), cookie, request))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            }
        }
    }
}
