using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class UsageTests : WithAutoFixture
    {
        private readonly KitosCredentials _apiUser;

        public UsageTests()
        {
            _apiUser = TestEnvironment.GetApiUser();
        }

        [Fact]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Usage_Data_From_Own_Organization()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_apiUser.Username, _apiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/Organizations(1)/ItSystemUsages"), token.Token))
            {
                var response = httpResponse.ReadListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(1, response.Result.First().Id);
                Assert.Single(response.Result);
            }
        }

        [Fact]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Usage_Data_From_Responsible_OrganizationUnit()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_apiUser.Username, _apiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/Organizations(1)/OrganizationUnits(1)/ItSystemUsages"), token.Token))
            {
                var response = httpResponse.ReadListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Empty(response.Result);
            }
        }
    }
}
