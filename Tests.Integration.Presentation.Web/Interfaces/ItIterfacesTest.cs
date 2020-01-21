using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Interfaces
{
    public class ItIterfacesTest : WithAutoFixture
    {
        private int _defaultUserId;

        [Fact]
        public async Task Global_Administrator_Can_Get_All_Interfaces()
        {
            //Arrange 
            var interFacePrefixName = CreateInterFacePrefixName();
            var interfacesCreated = await GenerateTestInterfaces(interFacePrefixName);
            var url = TestEnvironment.CreateUrl($"odata/ItInterfaces");
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                //Assert
                var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItInterface>();
                Assert.NotNull(response);
                var filteredResult = response.Where(x => x.Name.StartsWith(interFacePrefixName)).ToList();
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
            var expectedResults = interfacesCreated.Where(x =>
                (x.OrganizationId == orgId &&
                 (orgId == token.ActiveOrganizationId || role == OrganizationRole.GlobalAdmin)) ||
                x.AccessModifier == AccessModifier.Public).ToList();
            var url = TestEnvironment.CreateUrl($"/odata/Organizations({orgId})/ItInterfaces");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                //Assert
                var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItInterface>();
                Assert.NotNull(response);
                var filteredResult = response.Where(x => x.Name.StartsWith(interFacePrefixName)).ToList();
                Assert.Equal(expectedResults.Count, filteredResult.Count);
                Assert.True(expectedResults.Select(x => x.InterfaceId).SequenceEqual(filteredResult.Select(x => x.InterfaceId)));
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Can_Add_Data_Row(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), null, organizationId, AccessModifier.Public));

            //Act - perform the action with the actual role
            var result = await InterfaceHelper.CreateDataRowAsync(interfaceDto.Id, login);

            //Assert
            Assert.Equal(interfaceDto.Id, result.ItInterfaceId);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_Data_Row(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), null, organizationId, AccessModifier.Public));

            //Act - perform the action with the actual role
            using (var result = await InterfaceHelper.SendCreateDataRowRequestAsync(interfaceDto.Id, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Can_Set_Exposing_System(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), null, organizationId, AccessModifier.Public));
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);

            //Act - perform the action with the actual role
            var result = await InterfaceExhibitHelper.CreateExhibit(system.Id, interfaceDto.Id, login);

            //Assert
            Assert.Equal(interfaceDto.Id, result.ItInterfaceId);
            Assert.Equal(system.Id, result.ItSystemId);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Set_Exposing_System(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var interfaceDto = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), null, organizationId, AccessModifier.Public));
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);

            //Act - perform the action with the actual role
            using (var result = await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, interfaceDto.Id, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        private string CreateInterFacePrefixName()
        {
            return $"{nameof(ItIterfacesTest)}-{A<Guid>():N}";
        }

        private static async Task<Task<List<ItInterface>>> GetInterfacesByName(string name)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var arrangeUrl = TestEnvironment.CreateUrl($"/odata/ItInterfaces?$filter=contains(Name,'{name}')");
            using (var httpResponse = await HttpApi.GetWithTokenAsync(arrangeUrl, token.Token))
            {
                var response = httpResponse.ReadOdataListResponseBodyAsAsync<ItInterface>();
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
