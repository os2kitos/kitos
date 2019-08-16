using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageTests : WithAutoFixture
    {
        private readonly KitosCredentials _regularApiUser;

        public ItSystemUsageTests()
        {
            _regularApiUser = TestEnvironment.GetCredentials(OrganizationRole.User, true);
        }

        [Fact]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Usage_Data_From_Own_Organization()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_regularApiUser.Username, _regularApiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/Organizations(1)/ItSystemUsages"), token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
            }
        }

        [Fact]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Usage_Data_From_Responsible_OrganizationUnit()
        {
            //Arrange
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(_regularApiUser.Username, _regularApiUser.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/Organizations(1)/OrganizationUnits(1)/ItSystemUsages"), token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
            }
        }
    }
}
