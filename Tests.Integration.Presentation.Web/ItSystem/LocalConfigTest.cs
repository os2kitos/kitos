using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class LocalConfigTest : WithAutoFixture
    {
        [Fact]
        public async Task Cannot_Set_In_Other_Organization()
        {
            //Arrange
            var body = new
            {
                ShowColumnUsage = true
            };
            const int organizationId = TestEnvironment.SecondOrganizationId;

            //Act - perform the action with the actual role
            var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Set_If_Casing_Is_Wrong()
        {
            //Arrange
            var body = new
            {
                showColumnUsage = true
            };
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Can_Set_Column_Usage()
        {
            //Arrange
            var body = new
            {
                ShowColumnUsage = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_Tab_Overview()
        {
            //Arrange
            var body = new
            {
                ShowTabOverview = true
            };

            //Act + Assert
            await Can_Set(body);
        }
        
        [Fact]
        public async Task Can_Set_Column_Technology_Usage()
        {
            //Arrange
            var body = new
            {
                ShowColumnTechnology = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_Project_Prefix()
        {
            //Arrange
            var body = new
            {
                ShowItProjectPrefix = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_Project_Module()
        {
            //Arrange
            var body = new
            {
                ShowItProjectModule = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_System_Module()
        {
            //Arrange
            var body = new
            {
                ShowItSystemModule = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_System_Prefix()
        {
            //Arrange
            var body = new
            {
                ShowItSystemPrefix = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_Contract_Module()
        {
            //Arrange
            var body = new
            {
                ShowItContractModule = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_Contract_Prefix()
        {
            //Arrange
            var body = new
            {
                ShowItContractPrefix = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Config_Values_Are_Saved()
        {
            //Arrange
            var body = new
            {
                ShowColumnUsage = A<bool>(),
                ShowTabOverview = A<bool>(),
                ShowColumnTechnology = A<bool>(),
                ShowItProjectModule = A<bool>(),
                ShowItProjectPrefix = A<bool>(),
                ShowItSystemModule = A<bool>(),
                ShowItSystemPrefix = A<bool>(),
                ShowItContractModule = A<bool>(),
                ShowItContractPrefix = A<bool>()
            };
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var configResponse = await LocalConfigHelper.GetLocalConfig(organizationId);
            Assert.Equal(HttpStatusCode.OK, configResponse.StatusCode);
            var config = await configResponse.ReadResponseBodyAsAsync<Config>();
            Assert.Equal(body.ShowColumnUsage, config.ShowColumnUsage);
            Assert.Equal(body.ShowTabOverview, config.ShowTabOverview);
            Assert.Equal(body.ShowColumnTechnology, config.ShowColumnTechnology);
            Assert.Equal(body.ShowItProjectModule, config.ShowItProjectModule);
            Assert.Equal(body.ShowItProjectPrefix, config.ShowItProjectPrefix);
            Assert.Equal(body.ShowItSystemModule, config.ShowItSystemModule);
            Assert.Equal(body.ShowItSystemPrefix, config.ShowItSystemPrefix);
            Assert.Equal(body.ShowItContractModule, config.ShowItContractModule);
            Assert.Equal(body.ShowItContractPrefix, config.ShowItContractPrefix);
        }

        private static async Task Can_Set(object body)
        {
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

    }
}
