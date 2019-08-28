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

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_Organizations_That_Use_An_It_System(OrganizationRole apiUserType)
        {
            //Arrange
            var user = TestEnvironment.GetCredentials(apiUserType, true);
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(user.Username, user.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl("odata/ItSystemUsages"), token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.True(response.Result.Exists(x => x.OrganizationId == TestEnvironment.DefaultOrganizationId));
                Assert.True(response.Result.Exists(x => x.OrganizationId == TestEnvironment.SecondOrganizationId));
                Assert.NotEmpty(response.Result);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_Default_Organization_From_Default_It_System_Usage(OrganizationRole apiUserType)
        {
            //Arrange
            var user = TestEnvironment.GetCredentials(apiUserType, true);
            var loginDto = ObjectCreateHelper.MakeSimpleLoginDto(user.Username, user.Password);
            var token = await HttpApi.GetTokenAsync(loginDto);
            var url = TestEnvironment.CreateUrl(
                $"odata/ItSystemUsages?$expand=Organization&%24format=json&%24filter=ItSystemId+eq+{TestEnvironment.DefaultItSystemId}&%24count=true");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItSystemUsage>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.True(response.Result.Exists(x => x.OrganizationId == TestEnvironment.DefaultOrganizationId));
                Assert.NotEmpty(response.Result);
            }
        }

    }
}