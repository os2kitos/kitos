using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemCatalogTests : WithAutoFixture
    {

        [Theory]
        [InlineData(OrganizationRole.User)]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Api_Users_Can_Get_IT_System_Data_From_Specific_System_From_own_Organization(OrganizationRole role)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"odata/ItSystems({TestEnvironment.GetDefaultItSystemId()})");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.Result.Name);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.User, 1)]
        [InlineData(OrganizationRole.GlobalAdmin, 2)]
        public async Task Api_Users_Can_Get_All_IT_Systems_Data_From_Own_Organizations(OrganizationRole role, int minimumNumberOfItSystems)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystems"), token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.Result.First().Name);
                Assert.True(minimumNumberOfItSystems <= response.Result.Count);
            }
        }
    }
}
