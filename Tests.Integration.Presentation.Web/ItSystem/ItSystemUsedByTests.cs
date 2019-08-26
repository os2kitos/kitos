using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsedByTests : WithAutoFixture
    {
        private readonly KitosCredentials _globalAdminApiUser, _regularApiUser;

        public ItSystemUsedByTests()
        {
            _globalAdminApiUser = TestEnvironment.GetCredentials(OrganizationRole.GlobalAdmin, true);
            _regularApiUser = TestEnvironment.GetCredentials(OrganizationRole.User, true);
        }

        [Fact]
        public async Task Global_Admin_Api_User_Can_Get_Organizations_That_Use_An_It_System()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_globalAdminApiUser.Username, _globalAdminApiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystemUsages"), token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
            }
        }
        
        [Fact(Skip="Currently fails since the regular user can get information about other organizations which the user should not")]
        public async Task Regular_Api_User_Can_Not_Get_Information_About_Other_Organizations_That_Use_An_It_System()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_regularApiUser.Username, _regularApiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystemUsages"), token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.True(response.Result.All(x => x.OrganizationId == 1));
                Assert.NotEmpty(response.Result);
            }
        }

    }
}