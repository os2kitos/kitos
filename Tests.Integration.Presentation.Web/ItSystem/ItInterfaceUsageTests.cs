using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItInterfaceUsageTests : WithAutoFixture
    {

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_It_Interface_Usage(OrganizationRole role)
        {
            //Arrange
            await HttpApi.CreateItInterfaceUsageAsync(TestEnvironment.DefaultItSystemId, TestEnvironment.DefaultItInterfaceId, TestEnvironment.DefaultItSystemId, TestEnvironment.DefaultOrganizationId, TestEnvironment.DefaultContractId);
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"api/ItInterfaceUsage?usageId={TestEnvironment.DefaultItSystemId}&sysId={TestEnvironment.DefaultItSystemId}&interfaceId={TestEnvironment.DefaultItInterfaceId}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadResponseBodyAsKitosApiResponse<ItInterfaceUsageDTO>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(TestEnvironment.DefaultItSystemId, response.Result.ItSystemUsageId);
                Assert.Equal(TestEnvironment.DefaultItSystemId, response.Result.ItSystemId);
                Assert.Equal(TestEnvironment.DefaultItInterfaceId, response.Result.ItInterfaceId);
                Assert.Equal(TestEnvironment.DefaultContractId, response.Result.ItContractId);
            }
        }
    }
}
