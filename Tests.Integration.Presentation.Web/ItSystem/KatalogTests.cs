using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class KatalogTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser;

        public KatalogTests()
        {
            _apiUser = TestEnvironment.GetApiUser();
        }

        public async Task Api_User_Can_Get_IT_System_Data()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_apiUser.Username, _apiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystems(1)"), token.Token))
            {
                var response = HttpApi.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItSystem>(httpResponse);
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal("DefaultTestItSystem", response.Result.Name);
            }
            

        }
    }
}
