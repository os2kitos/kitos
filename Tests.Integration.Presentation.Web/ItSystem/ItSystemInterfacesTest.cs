using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemInterfacesTest : WithAutoFixture
    {
        [Theory]
        [InlineData(OrganizationRole.User)]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Api_Users_Can_Get_IT_Interfaces_Data_From_Own_Organization(OrganizationRole role)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"odata/Organizations({TestEnvironment.DefaultOrganizationId})/ItInterfaces");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, HttpStatusCode.OK)]
        [InlineData(OrganizationRole.User, HttpStatusCode.Forbidden)]
        public async Task Api_Global_Admin_Can_Get_All_ITsystem_Interfaces(OrganizationRole role,HttpStatusCode code)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"odata/ItInterfaces");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();
                //Assert
                Assert.Equal(code, httpResponse.StatusCode);
                Assert.NotNull(response);
            }
        }


        //  As a user i am able to see interfaces from my own organization and public from others

        public async Task User_Is_Able_To_Get_Interfaces_From_Own_Org_Or_Public()
        {

        }

        // As a global administrator i am able to see every interface created

        public async Task Global_Administrator_Can_Get_All_Interfaces()
        {

        }

        // As a user i am able to see a specific interface from my org or public 

        public async Task User_Is_Able_To_See_Specific_Interface_From_Own_Org_Or_public()
        {

        }

    }
}
