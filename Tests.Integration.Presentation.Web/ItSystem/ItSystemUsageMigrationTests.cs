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
            var itSystem = await ItSystemHelper.CreateItSystemInInitialOrganizationAsync(itSystemName, 1);

            var token = await HttpApi.GetTokenAsync(role);
            var orgId = 1;
            var url = TestEnvironment.CreateUrl($"api/ItSystemUsageMigration?organizationId={orgId}&nameContent={itSystemName}&numberOfItSystems={10}&getPublic={true}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<Core.DomainModel.ItSystem.ItSystem>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystems = response.ToList();
                Assert.NotEmpty(itSystems);
                Assert.Equal(1, itSystems.Count(x => x.Name == itSystemName));
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Api_User_Can_Limit_How_Many_Systems_To_Return(OrganizationRole role)
        {
            //Arrange
            var prefix = "migrationTestSystem";
            var itSystemName1 = prefix + CreateName();
            var itSystemName2 = prefix + CreateName();
            var itSystem1 = await ItSystemHelper.CreateItSystemInInitialOrganizationAsync(itSystemName1, 1);
            var itSystem2 = await ItSystemHelper.CreateItSystemInInitialOrganizationAsync(itSystemName2, 1);

            var token = await HttpApi.GetTokenAsync(role);
            var orgId = 1;
            var url = TestEnvironment.CreateUrl($"api/ItSystemUsageMigration?organizationId={orgId}&nameContent={prefix}&numberOfItSystems={2}&getPublic={true}");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<Core.DomainModel.ItSystem.ItSystem>>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                var itSystems = response.ToList();
                Assert.NotEmpty(itSystems);
                Assert.Equal(2, itSystems.Count);
            }
        }


        private static string CreateName()
        {
            return $"{Guid.NewGuid():N}";
        }
    }
}
