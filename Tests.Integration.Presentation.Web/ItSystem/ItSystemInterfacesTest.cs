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
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Global_Administrator_Can_Get_All_Interfaces(OrganizationRole role)
        {
            const string interFacePrefixName = "GlobalAdminInterfaceTest";
            
            //Arrange 
            var token = await HttpApi.GetTokenAsync(role);
            await GenerateTestInterfaces(interFacePrefixName);
            var interfaceResult = GetInterfacesByName(interFacePrefixName);
            var url = TestEnvironment.CreateUrl($"odata/ItInterfaces");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();
                var filteredResult = response.Result.Where(x => x.Name.StartsWith(interFacePrefixName));

                //Assert
                Assert.NotNull(response.Result);
                Assert.Equal(filteredResult.Count(), interfaceResult.Result.Result.Count);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, TestEnvironment.DefaultOrganizationId)]
        [InlineData(OrganizationRole.GlobalAdmin, TestEnvironment.SecondOrganizationId)]
        [InlineData(OrganizationRole.User, TestEnvironment.DefaultOrganizationId)]
        [InlineData(OrganizationRole.User, TestEnvironment.SecondOrganizationId)]
        public async Task User_Is_Able_To_Get_Interfaces_From_Own_Org_Or_Public(OrganizationRole role, int orgId)
        {
            const string interFacePrefixName = "UserInterfacesPreFix";
            var token = await HttpApi.GetTokenAsync(role);
            await GenerateTestInterfaces(interFacePrefixName);
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
                        Assert.Equal(AccessModifier.Public, item.AccessModifier);
                    }
                }
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task User_Is_Able_To_See_Specific_Interface_From_Own_Org_Or_public(OrganizationRole role)
        {
            const string interFacePrefixName = "UserSpecificInterfacePrefix";
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            await GenerateTestInterfaces(interFacePrefixName);
            var interfaceResultByName = await GetInterfacesByName(interFacePrefixName);

            foreach (var item in interfaceResultByName.Result)
            {
                var orgFromItem = item.OrganizationId;
                var interFaceId = item.Id;

                var url = TestEnvironment.CreateUrl($"odata/Organizations({orgFromItem})/ItInterfaces({interFaceId})");

                //Act
                using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
                {
                    var response = httpResponse.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();
                    //Assert
                    Assert.NotNull(interfaceResultByName);
                    Assert.Equal(interFaceId, response.Result.Id);
                }
            }
        }
        // Return wrong number of interfaces.....
        private async Task<Task<List<ItInterface>>> GetInterfacesByName(string name)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var arrangeUrl = TestEnvironment.CreateUrl($"/odata/ItInterfaces?$filter=contains(Name,'{name}')");
            using (var httpResponse = await HttpApi.GetWithTokenAsync(arrangeUrl, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItInterface>();
                return response;
            }
        }

        private async Task GenerateTestInterfaces(string name)
        {
            await InterfaceHelper.CreateInterfaces(
                InterfaceHelper.CreateInterfaceDTO(name + "-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Local),
                InterfaceHelper.CreateInterfaceDTO(name + "-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public),
                InterfaceHelper.CreateInterfaceDTO(name + "-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, TestEnvironment.SecondOrganizationId, AccessModifier.Local),
                InterfaceHelper.CreateInterfaceDTO(name + "-" + A<Guid>(), A<Guid>().ToString(), TestEnvironment.DefaultUserId, TestEnvironment.SecondOrganizationId, AccessModifier.Public));
        }
    }
}
