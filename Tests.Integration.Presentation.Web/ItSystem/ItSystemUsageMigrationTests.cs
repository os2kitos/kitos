using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageMigrationTests
    {

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Get_Public_Or_Own_Organization_It_Systems_Not_In_Use_In_Own_Organization(OrganizationRole role)
        {
            //Arrange
            var itSystemName = CreateName();
            var itSystem = await HttpApi.CreateItSystemInInitialOrganizationAsync(itSystemName);

            var token = await HttpApi.GetTokenAsync(role);
            var orgId = 1;
            var url = TestEnvironment.CreateUrl($"api/ItSystemUsageMigration?organizationId={orgId}&nameContent={itSystemName[0]}&limit={10}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadResponseBodyAsKitosApiResponse<IEnumerable<Core.DomainModel.ItSystem.ItSystem>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotEmpty(response.Result);
                Assert.Equal(1, response.Result.Count(x => x.Name == itSystemName));
            }
        }


        private static string CreateName()
        {
            return $"{Guid.NewGuid():N}";
        }
    }
}
