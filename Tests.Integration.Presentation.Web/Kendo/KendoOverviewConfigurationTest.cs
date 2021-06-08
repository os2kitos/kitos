using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Kendo
{
    public class KendoOverviewConfigurationTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Save_Configuration()
        {
            //Arrange
            var overviewType = A<OverviewType>();
            var config = A<string>();

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, config);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var kendoConfig = await response.ReadResponseBodyAsKitosApiResponseAsync<KendoOrganizationalConfigurationDTO>();
            Assert.Equal(TestEnvironment.DefaultOrganizationId, kendoConfig.OrganizationId);
            Assert.Equal(overviewType, kendoConfig.OverviewType);
            Assert.Equal(config, kendoConfig.Configuration);
        }

        [Fact]
        public async Task Can_Get_Configuration()
        {
            //Arrange
            var overviewType = A<OverviewType>();
            var config = A<string>();

            var saveResponse = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, config);
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendGetConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var kendoConfig = await response.ReadResponseBodyAsKitosApiResponseAsync<KendoOrganizationalConfigurationDTO>();
            Assert.Equal(TestEnvironment.DefaultOrganizationId, kendoConfig.OrganizationId);
            Assert.Equal(overviewType, kendoConfig.OverviewType);
            Assert.Equal(config, kendoConfig.Configuration);
        }

        [Fact]
        public async Task Can_Delete_Configuration()
        {
            //Arrange
            var overviewType = A<OverviewType>();
            var config = A<string>();

            var saveResponse = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, config);
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendDeleteConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Not_Get_Configuration_If_None_Exists()
        {
            //Arrange
            var orgId = int.MaxValue;
            var overviewType = A<OverviewType>();

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendGetConfigurationRequestAsync(orgId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Can_Not_Save_Configuration_If_Not_Local_Admin(OrganizationRole orgRole)
        {
            //Arrange
            var overviewType = A<OverviewType>();
            var config = A<string>();
            var cookie = await HttpApi.GetCookieAsync(orgRole);

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, config, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Can_Not_Delete_Configuration_If_None_Exists()
        {
            //Arrange
            var orgId = int.MaxValue;
            var overviewType = A<OverviewType>();

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendDeleteConfigurationRequestAsync(orgId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Can_Not_Delete_Configuration_If_Not_Local_Admin(OrganizationRole orgRole)
        {
            //Arrange
            var overviewType = A<OverviewType>();
            var config = A<string>();
            var saveResponse = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, config);
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

            var cookie = await HttpApi.GetCookieAsync(orgRole);

            //Act
            var response = await KendoOverviewConfigurationHelper.SendDeleteConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
