﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Presentation.Web.Models.Result;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemTests : WithAutoFixture
    {

        [Theory]
        [InlineData(OrganizationRole.User)]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Api_Users_Can_Get_IT_System_Data_From_Specific_System_From_own_Organization(OrganizationRole role)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(role);
            var url = TestEnvironment.CreateUrl($"odata/ItSystems({TestEnvironment.DefaultItSystemId})");

            //Act
            using (var httpResponse = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                var response = await httpResponse.ReadResponseBodyAsAsync<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.Name);
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
                var response = await httpResponse.ReadOdataListResponseBodyAsAsync<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.First().Name);
                Assert.True(minimumNumberOfItSystems <= response.Count);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, false)]
        [InlineData(OrganizationRole.User, true, false)]
        public async Task GetAccessRights_Returns(OrganizationRole role, bool canView, bool canCreate)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using (var httpResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/itsystem?getEntitiesAccessRights=true"), cookie))
            {
                //Assert
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<EntitiesAccessRightsDTO>();
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(canView, response.CanView);
                Assert.Equal(canCreate, response.CanCreate);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, true)] //Local admin in own org can delete itsystem
        [InlineData(OrganizationRole.User, true, false, false)]
        public async Task GetAccessRightsForEntity_Returns(OrganizationRole role, bool canView, bool canEdit, bool canDelete)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using (var httpResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/itsystem?id={TestEnvironment.DefaultItSystemId}&getEntityAccessRights=true"), cookie))
            {
                //Assert
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<EntityAccessRightsDTO>();
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(canView, response.CanView);
                Assert.Equal(canEdit, response.CanEdit);
                Assert.Equal(canDelete, response.CanDelete);
            }
        }

        [Fact]
        public async Task GetAccessRightsForEntity_With_Unknown_Entity_Returns_404()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            //Act
            using (var httpResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/itsystem?id=-1&getEntityAccessRights=true"), cookie))
            {
                //Assert
                Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Add_Data_Worker(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);

            //Act - perform the action with the actual role
            var result = await ItSystemHelper.SetDataWorkerAsync(system.Id, organizationId, optionalLogin: login);

            //Assert
            Assert.Equal(organizationId, result.DataWorkerId);
            Assert.Equal(system.Id, result.ItSystemId);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Add_SystemUsage_Data_Worker(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);

            //Act
            using (var result = await ItSystemHelper.SendSetDataWorkerRequestAsync(system.Id, organizationId, optionalLogin: login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Delete_System(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);

            //Act
            using (var result = await ItSystemHelper.DeleteItSystemAsync(system.Id, organizationId, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                AssertSystemDeletedAsync(system.Id);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Delete_System(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);

            //Act
            using (var result = await ItSystemHelper.DeleteItSystemAsync(system.Id, organizationId, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
                await AssertSystemNotDeletedAsync(system.Id);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Cannot_Delete_System_In_Use(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Act
            using (var result = await ItSystemHelper.DeleteItSystemAsync(system.Id, organizationId, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
                var resultValue = await result.ReadResponseBodyAsKitosApiResponseAsync<string>();
                Assert.Equal(SystemDeleteConflict.InUse.ToString("G"), resultValue);
                await AssertSystemNotDeletedAsync(system.Id);
            }
        }


        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Cannot_Delete_System_With_Child_Systems(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var mainSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var childSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            await ItSystemHelper.SetParentSystem(childSystem.Id, mainSystem.Id, organizationId, login);

            //Act
            using (var result = await ItSystemHelper.DeleteItSystemAsync(mainSystem.Id, organizationId, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
                var resultValue = await result.ReadResponseBodyAsKitosApiResponseAsync<string>();
                Assert.Equal(SystemDeleteConflict.HasChildren.ToString("G"), resultValue);
                await AssertSystemNotDeletedAsync(mainSystem.Id);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Cannot_Delete_System_With_Interface_Exhibits(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var itInterfaceDto = InterfaceHelper.CreateInterfaceDto(
                A<string>(), 
                A<string>(), 
                null, 
                organizationId,
                AccessModifier.Public);
            var itInterface = await InterfaceHelper.CreateInterface(itInterfaceDto);
            await InterfaceExhibitHelper.CreateExhibit(system.Id, itInterface.Id);

            //Act
            using (var result = await ItSystemHelper.DeleteItSystemAsync(system.Id, organizationId, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
                var resultValue = await result.ReadResponseBodyAsKitosApiResponseAsync<string>();
                Assert.Equal(SystemDeleteConflict.HasInterfaceExhibits.ToString("G"), resultValue);
                await AssertSystemNotDeletedAsync(system.Id);
            }
        }

        private static async Task AssertSystemNotDeletedAsync(int systemId)
        {
            var result = await ItSystemHelper.GetSystem(systemId);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        private static async Task AssertSystemDeletedAsync(int systemId)
        {
            var result = await ItSystemHelper.GetSystem(systemId);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
