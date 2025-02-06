using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageTests : WithAutoFixture
    {
        [Fact]
        public async Task GlobalAdmin_User_Can_Get_Usages_Across_Organizations()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var httpResponse = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl("odata/ItSystemUsages"), cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var response = await httpResponse.ReadOdataListResponseBodyAsAsync<ItSystemUsage>();
            Assert.True(response.Exists(x => x.OrganizationId == TestEnvironment.DefaultOrganizationId));
            Assert.True(response.Exists(x => x.OrganizationId == TestEnvironment.SecondOrganizationId));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Can_Get_Default_Organization_From_Default_It_System_Usage(OrganizationRole role)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(role);
            var url = TestEnvironment.CreateUrl(
                $"odata/ItSystemUsages({TestEnvironment.DefaultItSystemId})");

            //Act
            using var httpResponse = await HttpApi.GetWithCookieAsync(url, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var response = await httpResponse.ReadResponseBodyAsAsync<ItSystemUsage>();
            Assert.True(response.OrganizationId == TestEnvironment.DefaultOrganizationId);
        }

        [Fact]
        public async Task Can_Change_User_Count()
        {
            //Arrange
            var newSystemName = A<string>();
            var accessModifier = AccessModifier.Public;
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(newSystemName, TestEnvironment.DefaultOrganizationId, accessModifier);
            var newSystemUsage = new ItSystemUsage()
            {
                OrganizationId = system.OrganizationId,
                ItSystemId = system.Id
            };
            var itSystemUsage = ItSystemUsageHelper.CreateItSystemUsage(newSystemUsage);

            //Act
            var patchObject = new { UserCount = EnumRange.All<UserCount>().RandomItem() };
            await ItSystemUsageHelper.PatchSystemUsage(itSystemUsage.Id, itSystemUsage.OrganizationId, patchObject);

            //Assert
            var getPatchedItSystemUsage = await ItSystemUsageHelper.GetItSystemUsageRequestAsync(itSystemUsage.Id);
            Assert.Equal(itSystemUsage.Id, getPatchedItSystemUsage.Id);
            Assert.Equal(patchObject.UserCount, getPatchedItSystemUsage.UserCount);
        }
    }
}
