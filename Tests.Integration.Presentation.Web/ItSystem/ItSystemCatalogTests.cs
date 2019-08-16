using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemCatalogTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser;

        public ItSystemCatalogTests()
        {
            _apiUser = TestEnvironment.GetApiUser();
        }

        [Fact]
        public async Task Api_User_Can_Get_IT_System_Data_From_Specific_System_From_own_Organization()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_apiUser.Username, _apiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystems(1)"), token.Token))
            {
                var response = httpResponse.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.Result.Name);
            }
        }

        [Fact]
        public async Task Api_User_Can_Get_All_IT_Systems_Data_From_Own_Organizations()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_apiUser.Username, _apiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystems"), token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.Result.First().Name);
            }
        }
    }
}
