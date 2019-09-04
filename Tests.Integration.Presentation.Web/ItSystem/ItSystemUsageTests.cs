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

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Use_Data_From_Own_Organization(OrganizationRole role)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"odata/Organizations({TestEnvironment.DefaultOrganizationId})/ItSystemUsages");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_All_IT_Systems_In_Use_Data_From_Responsible_OrganizationUnit(OrganizationRole role)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"odata/Organizations({TestEnvironment.DefaultOrganizationId})/OrganizationUnits({TestEnvironment.DefaultOrganizationId})/ItSystemUsages");
            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response);
            }
        }

        [Fact]
        public async Task Api_GlobalAdmin_User_Can_Get_Usages_Across_Organizations()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystemUsages"), token.Token))
            {
                var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.True(response.Exists(x => x.OrganizationId == TestEnvironment.DefaultOrganizationId));
                Assert.True(response.Exists(x => x.OrganizationId == TestEnvironment.SecondOrganizationId));
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_Default_Organization_From_Default_It_System_Usage(OrganizationRole role)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl(
                $"odata/ItSystemUsages({TestEnvironment.DefaultItSystemId})");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsAsync<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.True(response.OrganizationId == TestEnvironment.DefaultOrganizationId);
            }
        }
    }
}
