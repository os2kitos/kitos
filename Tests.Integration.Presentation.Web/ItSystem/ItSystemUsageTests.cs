using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageTests : WithAutoFixture
    {

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Can_Get_All_IT_Systems_In_Use_Data_From_Own_Organization(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl($"odata/Organizations({TestEnvironment.DefaultOrganizationId})/ItSystemUsages");

            //Act
            using var httpResponse = await HttpApi.GetWithCookieAsync(url, cookie);
            
            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItSystemUsage>();
            Assert.NotEmpty(response);
        }

        [Fact]
        public async Task GlobalAdmin_User_Can_Get_Usages_Across_Organizations()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var httpResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("odata/ItSystemUsages"), cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItSystemUsage>();
            Assert.True(response.Exists(x => x.OrganizationId == TestEnvironment.DefaultOrganizationId));
            Assert.True(response.Exists(x => x.OrganizationId == TestEnvironment.SecondOrganizationId));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Can_Get_Default_Organization_From_Default_It_System_Usage(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl(
                $"odata/ItSystemUsages({TestEnvironment.DefaultItSystemId})");

            //Act
            using var httpResponse = await HttpApi.GetWithCookieAsync(url, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var response = await httpResponse.ReadResponseBodyAsAsync<ItSystemUsage>();
            Assert.True(response.OrganizationId == TestEnvironment.DefaultOrganizationId);
        }
    }
}
