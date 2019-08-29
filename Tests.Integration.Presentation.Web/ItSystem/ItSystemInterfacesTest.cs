using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemInterfacesTest : WithAutoFixture
    {
        //  As a user i am able to see interfaces from my own organization and public from others
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Global_Administrator_Can_Get_All_Interfaces(OrganizationRole role)
        {
            //Arrange Global_Administrator_Can_Get_All_Interfaces
            var token = await HttpApi.GetTokenAsync(role);

            await GenerateTestInterfaces();

            var res = GetInterfacesByName("IntegrationTest");

            var url = TestEnvironment.CreateUrl($"odata/ItInterfaces");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();

                //Assert
                Assert.NotNull(response.Result);
                Assert.Equal(response.Result.Count, res.Result.Result.Count);
            }
        }

        // As a global administrator i am able to see every interface created
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, 1)]
        [InlineData(OrganizationRole.GlobalAdmin, 2)]
        [InlineData(OrganizationRole.User, 1)]
        [InlineData(OrganizationRole.User, 2)]
        public async Task User_Is_Able_To_Get_Interfaces_From_Own_Org_Or_Public(OrganizationRole role, int orgId)
        {
            var token = await HttpApi.GetTokenAsync(role);

            await GenerateTestInterfaces();

            var url = TestEnvironment.CreateUrl($"/odata/Organizations({orgId})/ItInterfaces");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();

                //Assert
                Assert.NotNull(response.Result);

                foreach (var item in response.Result)
                {
                    if (item.OrganizationId != orgId)
                    {
                        Assert.NotEqual(item.AccessModifier, AccessModifier.Local);
                    }
                }
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task User_Is_Able_To_See_Specific_Interface_From_Own_Org_Or_public(OrganizationRole role)
        {
            var token = await HttpApi.GetTokenAsync(role);

            await GenerateTestInterfaces();

            var res = await GetInterfacesByName("IntegrationTest");

            foreach (var item in res.Result)
            {

                var orgFromItem = item.OrganizationId;
                var interFaceId = item.Id;

                var url = TestEnvironment.CreateUrl($"odata/Organizations({orgFromItem})/ItInterfaces({interFaceId})");

                //Act
                using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
                {
                    var response = httpResponse.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();
                    //Assert
                    Assert.NotNull(res);
                    Assert.Equal(interFaceId, response.Result.Id);
                }
            }
        }


        public async Task<Task<List<ItInterface>>> GetInterfacesByName(string name)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var arrangeUrl = TestEnvironment.CreateUrl($"odata/ItInterfaces?filter=contains({name})");
            using (var httpResponse = await HttpApi.GetWithTokenAsync(arrangeUrl, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();
                return response;
            }
        }

        public async Task GenerateTestInterfaces()
        {
            await InterfaceHelper.CreateInterfaces(
                InterfaceHelper.CreateInterfaceDTO("IntegrationTest-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Local),
                InterfaceHelper.CreateInterfaceDTO("IntegrationTest-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public),
                InterfaceHelper.CreateInterfaceDTO("IntegrationTest-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, 2, AccessModifier.Local),
                InterfaceHelper.CreateInterfaceDTO("IntegrationTest-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, 2, AccessModifier.Public));
        }
    }
}
