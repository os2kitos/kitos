using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Newtonsoft.Json.Linq;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Security
{
    [CollectionDefinition("Non-parallel Collection", DisableParallelization = true)]
    public class TokenInvalidationTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser, _globalAdmin;

        public TokenInvalidationTests()
        {
            _apiUser = TestEnvironment.GetCredentials(OrganizationRole.ApiAccess);
            _globalAdmin = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin);
        }

        //[Fact]
        public async Task Token_Can_Be_Invalidated_After_Creation()
        {
            var globalRole = _globalAdmin.Role;
            var cookie = await HttpApi.GetCookieAsync(globalRole);

            var apiRole = _apiUser.Role;
            var token = await HttpApi.GetTokenAsync(apiRole);

            using (var requestResponse =
                await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);
            };

            var rights = await HttpApi.GetAsync(TestEnvironment.CreateUrl("odata/Organizations(1)/Rights"));
            var rightsAsString = rights.Content.ReadAsStringAsync();
            var json = JObject.Parse(rightsAsString.Result);
            var idOfRightToDelete = default(int);
            foreach (var rightsElements in json.Last.First)
            {
                if (rightsElements.Value<string>("Role") == "ApiAccess")
                {
                    idOfRightToDelete = rightsElements.Value<int>("Id");
                }
            }
            var delete = await HttpApi.DeleteAsyncWithCookie(TestEnvironment.CreateUrl($"odata/Organizations(1)/Rights({idOfRightToDelete})"), cookie);
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);


            using (var requestResponse =
                await HttpApi.GetAsyncWithToken(TestEnvironment.CreateUrl("api/ItSystem/"), token.Token))
            {
                Assert.NotNull(requestResponse);
                Assert.Equal(HttpStatusCode.Forbidden, requestResponse.StatusCode);
            };

            var orgRightDTO = new OrgRightDTO()
            {
                Role = OrganizationRole.ApiAccess.ToString(),
                UserId = 5.ToString(),
                OrganizationId = 1.ToString()
            };

            var resp = await HttpApi.PostAsyncWithCookie(TestEnvironment.CreateUrl("odata/Organizations(1)/Rights"), cookie,
                orgRightDTO);

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        }
    }
}
