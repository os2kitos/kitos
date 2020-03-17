using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using ExpectedObjects.Comparisons;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageGDPRTest : WithAutoFixture
    {

        [Fact]
        public async Task Can_Change_DataOptions()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new {IsBusinessCritical = dataOption};
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);

            //Act
            var itSystemUsageDTO = await ItSystemUsageHelper.PatchSystemUsage(usage.Id, organizationId, body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.IsBusinessCritical);
            Assert.Equal(dataOption, itSystemUsageDTO.IsBusinessCritical.Value);

        }

        [Fact]
        public async Task Can_Remove_SensitiveDataLevel()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);
            var sensitivityLevel = (int)SensitiveDataLevel.NONE;
            await ItSystemUsageHelper.AddSensitiveDataLevel(usage.Id, sensitivityLevel);

            //Act
            var sensitivityLevelDTO =
                await ItSystemUsageHelper.RemoveSensitiveDataLevel(usage.Id, sensitivityLevel);

            //Assert
            Assert.Equal(sensitivityLevel, sensitivityLevelDTO.DataSensitivityLevel);
            var updatedUsage = await ItSystemHelper.GetItSystemUsage(usage.Id);
            Assert.Empty(updatedUsage.SensitiveDataLevels);
        }

        [Fact]
        public async Task Cannot_Add_SensitiveDataLevel_If_Level_Already_Exists()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);
            var sensitivityLevel = (int)SensitiveDataLevel.NONE;
            await ItSystemUsageHelper.AddSensitiveDataLevel(usage.Id, sensitivityLevel);

            //Act
            using (var result = await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v1/itsystemusage/{usage.Id}/sensitivityLevel/add/{sensitivityLevel}"), cookie, null))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
                var notUpdatedUsage = await ItSystemHelper.GetItSystemUsage(usage.Id);
                var sensitiveDataLevel = Assert.Single(notUpdatedUsage.SensitiveDataLevels);
                Assert.Equal(sensitivityLevel, sensitiveDataLevel.DataSensitivityLevel);
            }
        }

        [Fact]
        public async Task Cannot_Remove_SensitiveDataLevel_If_Level_Does_Not_Exists()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);
            var sensitivityLevel = (int)SensitiveDataLevel.NONE;

            //Act
            using (var result = await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v1/itsystemusage/{usage.Id}/sensitivityLevel/remove/{sensitivityLevel}"), cookie, null))
            {
                //Assert
                Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
                var notUpdatedUsage = await ItSystemHelper.GetItSystemUsage(usage.Id);
                Assert.Empty(notUpdatedUsage.SensitiveDataLevels);
            }
        }




    }
}
