using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _defaultUserId;

        [Fact]
        public async Task Global_Administrator_Can_Get_All_Interfaces()
        {
            //Arrange 
            var interFacePrefixName = CreateInterFacePrefixName();
            var interfacesCreated = await GenerateTestInterfaces(interFacePrefixName);
            //var interfaceResult = GetInterfacesByName(interFacePrefixName);
            var url = TestEnvironment.CreateUrl($"odata/ItInterfaces");
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItInterface>();
                var filteredResult = response.Result.Where(x => x.Name.StartsWith(interFacePrefixName)).ToList();

                //Assert
                Assert.NotNull(response.Result);
                Assert.Equal(interfacesCreated.Length, filteredResult.Count);
                Assert.True(interfacesCreated.Select(x => x.InterfaceId).SequenceEqual(filteredResult.Select(x => x.InterfaceId)));
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, TestEnvironment.DefaultOrganizationId)]
        [InlineData(OrganizationRole.GlobalAdmin, TestEnvironment.SecondOrganizationId)]
        [InlineData(OrganizationRole.User, TestEnvironment.DefaultOrganizationId)]
        [InlineData(OrganizationRole.User, TestEnvironment.SecondOrganizationId)]
        public async Task User_Is_Able_To_Get_Interfaces_From_Own_Org_Or_Public(OrganizationRole role, int orgId)
        {
            //Arrabge
            var interFacePrefixName = CreateInterFacePrefixName();
            var token = await HttpApi.GetTokenAsync(role);
            var interfacesCreated = await GenerateTestInterfaces(interFacePrefixName);
            var expectedResults = interfacesCreated.Where(x => x.OrganizationId == orgId || x.AccessModifier == AccessModifier.Public).ToList();
            var url = TestEnvironment.CreateUrl($"/odata/Organizations({orgId})/ItInterfaces");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItInterface>();
                var filteredResult = response.Result.Where(x => x.Name.StartsWith(interFacePrefixName)).ToList();

                //Assert
                Assert.NotNull(response.Result);

                Assert.Equal(expectedResults.Count, filteredResult.Count);
                Assert.True(expectedResults.Select(x => x.InterfaceId).SequenceEqual(filteredResult.Select(x => x.InterfaceId)));
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task User_Is_Able_To_See_Specific_Interface_From_Own_Org_Or_public(OrganizationRole role)
        {
            //Arrange
            var interFacePrefixName = CreateInterFacePrefixName();
            var token = await HttpApi.GetTokenAsync(role);
            await GenerateTestInterfaces(interFacePrefixName);
            var interfaceResultByName = await GetInterfacesByName(interFacePrefixName);

            foreach (var item in interfaceResultByName.Result)
            {
                var orgFromItem = item.OrganizationId;
                var key = item.Id;

                var url = TestEnvironment.CreateUrl($"odata/Organizations({orgFromItem})/ItInterfaces({key})");

                //Act
                using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
                {
                    var response = httpResponse.ReadResponseBodyAs<ItInterface>();
                    //Assert
                    Assert.NotNull(interfaceResultByName);
                    Assert.Equal(key, response.Result.Id);
                }
            }
        }

        private string CreateInterFacePrefixName()
        {
            return $"{nameof(ItSystemInterfacesTest)}-{A<Guid>():N}";
        }

        private static async Task<Task<List<ItInterface>>> GetInterfacesByName(string name)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var arrangeUrl = TestEnvironment.CreateUrl($"/odata/ItInterfaces?$filter=contains(Name,'{name}')");
            using (var httpResponse = await HttpApi.GetWithTokenAsync(arrangeUrl, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAs<ItInterface>();
                return response;
            }
        }

        private async Task<ItInterfaceDTO[]> GenerateTestInterfaces(string name)
        {
            _defaultUserId = TestEnvironment.DefaultUserId;
            var itInterfaceDto1 = InterfaceHelper.CreateInterfaceDto($"{name}-{A<Guid>():N}", A<Guid>().ToString(), _defaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterfaceDto2 = InterfaceHelper.CreateInterfaceDto($"{name}-{A<Guid>():N}", A<Guid>().ToString(), _defaultUserId, TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var itInterfaceDto3 = InterfaceHelper.CreateInterfaceDto($"{name}-{A<Guid>():N}", A<Guid>().ToString(), _defaultUserId, TestEnvironment.SecondOrganizationId, AccessModifier.Local);
            var itInterfaceDto4 = InterfaceHelper.CreateInterfaceDto($"{name}-{A<Guid>():N}", A<Guid>().ToString(), _defaultUserId, TestEnvironment.SecondOrganizationId, AccessModifier.Public);
            await InterfaceHelper.CreateInterfaces(itInterfaceDto1, itInterfaceDto2, itInterfaceDto3, itInterfaceDto4);
            return new[]
            {
                itInterfaceDto1,
                itInterfaceDto2,
                itInterfaceDto3,
                itInterfaceDto4
            };
        }
    }
}
