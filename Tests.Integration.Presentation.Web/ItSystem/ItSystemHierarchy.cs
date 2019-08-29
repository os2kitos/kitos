using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemHierarchy
    {

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_It_System_Hierarchy_Information(OrganizationRole role)
        {
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"api/itsystem/{TestEnvironment.DefaultItSystemId}?hierarchy=true");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadResponseBodyAsKitosApiResponse<IEnumerable<ItSystemDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Api_User_Can_Get_It_System_ParentId(OrganizationRole role)
        {
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"api/itsystem/{TestEnvironment.SecondItSystemId}?hierarchy=true");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadResponseBodyAsKitosApiResponse<IEnumerable<ItSystemDTO>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
                Assert.Equal(TestEnvironment.DefaultItSystemId, response.Result.Where(x => x.Id == TestEnvironment.SecondItSystemId).First().ParentId);
            }
        }
    }
}
