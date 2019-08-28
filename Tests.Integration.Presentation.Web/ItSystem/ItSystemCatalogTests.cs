using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemCatalogTests : WithAutoFixture
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
                var response = httpResponse.ReadResponseBodyAs<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.Result.Name);
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
                var response = httpResponse.ReadOdataListResponseBodyAs<Core.DomainModel.ItSystem.ItSystem>();
                //Assert
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.NotNull(response.Result.First().Name);
                Assert.True(minimumNumberOfItSystems <= response.Result.Count);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, false, true, false)]
        [InlineData(OrganizationRole.LocalAdmin, true, false, false, false)]
        [InlineData(OrganizationRole.User, true, false, false, false)]
        public async Task GetAccessRights_Returns(OrganizationRole role, bool canView, bool canEdit, bool canCreate, bool canDelete)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using (var httpResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("api/itsystem?getEntitiesAccessRights=true"), cookie))
            {
                //Assert
                var response = httpResponse.ReadResponseBodyAsKitosApiResponse<EntitiesAccessRightsDTO>();
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(canView, response.Result.CanView);
                Assert.Equal(canCreate, response.Result.CanCreate);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, false, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, false, true)] //Local admin in own org can delete itsystem
        [InlineData(OrganizationRole.User, true, false, false, false)]
        public async Task GetAccessRightsForEntity_Returns(OrganizationRole role, bool canView, bool canEdit, bool canCreate, bool canDelete)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);

            //Act
            using (var httpResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/itsystem?id={TestEnvironment.DefaultItSystemId}&getEntityAccessRights=true"), cookie))
            {
                //Assert
                var response = httpResponse.ReadResponseBodyAsKitosApiResponse<EntityAccessRightsDTO>();
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                Assert.Equal(canView, response.Result.CanView);
                Assert.Equal(canEdit, response.Result.CanEdit);
                Assert.Equal(canDelete, response.Result.CanDelete);
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
    }
}
