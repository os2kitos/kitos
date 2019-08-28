using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageTests : WithAutoFixture
    {

        [Fact]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Use_Data_From_Own_Organization()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var url = TestEnvironment.CreateUrl($"odata/Organizations({TestEnvironment.DefaultOrganizationId})/ItSystemUsages");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
            }
        }

        [Fact]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Use_Data_From_Responsible_OrganizationUnit()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var url = TestEnvironment.CreateUrl($"odata/Organizations({TestEnvironment.DefaultOrganizationId})/OrganizationUnits({TestEnvironment.DefaultOrganizationId})/ItSystemUsages");
            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
            }
        }
    }
}
