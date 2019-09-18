using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class LocalConfigTest : WithAutoFixture
    {

        [Fact]
        public async Task Can_Set_Column_Usage()
        {
            //Arrange
            var body = new
            {
                ShowColumnUsage = true
            };
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
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
        public async Task Can_Set_Tab_Overview()
        {
            //Arrange
            var body = new
            {
                ShowTabOverview = true
            };
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task Can_Set_Column_Technology_Usage()
        {
            //Arrange
            var body = new
            {
                ShowColumnTechnology = true
            };
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }
    }
}
