using System;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageGDPRTest : WithAutoFixture
    {

        [Fact]
        public async Task Can_Change_IsBusinessCritical()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new {IsBusinessCritical = dataOption};
            
            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.IsBusinessCritical);
            Assert.Equal(dataOption, itSystemUsageDTO.IsBusinessCritical.Value);
        }

        [Fact]
        public async Task Can_Change_DataProcessorControl()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new { DataProcessorControl = dataOption };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.DataProcessorControl);
            Assert.Equal(dataOption, itSystemUsageDTO.DataProcessorControl.Value);
        }

        [Fact]
        public async Task Can_Change_Precautions()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new { Precautions = dataOption };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.Precautions);
            Assert.Equal(dataOption, itSystemUsageDTO.Precautions.Value);
        }

        [Fact]
        public async Task Can_Change_RiskAssessment()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new { RiskAssessment = dataOption };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.RiskAssessment);
            Assert.Equal(dataOption, itSystemUsageDTO.RiskAssessment.Value);
        }

        [Fact]
        public async Task Can_Change_PreRiskAssessment()
        {
            //Arrange
            var riskLevel = A<RiskLevel>();
            var body = new { PreRiskAssessment = riskLevel };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.PreRiskAssessment);
            Assert.Equal(riskLevel, itSystemUsageDTO.PreRiskAssessment.Value);
        }

        [Fact]
        public async Task Can_Change_DPIA()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new { DPIA = dataOption };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.DPIA);
            Assert.Equal(dataOption, itSystemUsageDTO.DPIA.Value);
        }

        [Fact]
        public async Task Can_Change_AnsweringDataDPIA()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new { AnsweringDataDPIA = dataOption };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.AnsweringDataDPIA);
            Assert.Equal(dataOption, itSystemUsageDTO.AnsweringDataDPIA.Value);
        }

        [Fact]
        public async Task Can_Change_UserSupervision()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new { UserSupervision = dataOption };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.UserSupervision);
            Assert.Equal(dataOption, itSystemUsageDTO.UserSupervision.Value);
        }


        private async Task<ItSystemUsageDTO> Create_System_Usage_And_Change_Value_By_Body(Object body)
        {
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);
            return await ItSystemUsageHelper.PatchSystemUsage(usage.Id, organizationId, body);
        }

        [Fact]
        public async Task Can_Add_SensitiveDataLevel()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);
            var sensitivityLevel = A<SensitiveDataLevel>();

            //Act
            var sensitivityLevelDTO =
                await ItSystemUsageHelper.AddSensitiveDataLevel(usage.Id, sensitivityLevel);

            //Assert
            Assert.Equal(sensitivityLevel, sensitivityLevelDTO.DataSensitivityLevel);
        }

        [Fact]
        public async Task Can_Remove_SensitiveDataLevel()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);
            var sensitivityLevel = A<SensitiveDataLevel>();
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
            var sensitivityLevel = A<SensitiveDataLevel>();
            await ItSystemUsageHelper.AddSensitiveDataLevel(usage.Id, sensitivityLevel);

            //Act
            using (var result = await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v1/itsystemusage/{usage.Id}/sensitivityLevel/add"), cookie, sensitivityLevel))
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

            //Act
            using (var result = await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl(
                    $"api/v1/itsystemusage/{usage.Id}/sensitivityLevel/remove"), cookie, A<SensitiveDataLevel>()))
            {
                //Assert
                Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                var notUpdatedUsage = await ItSystemHelper.GetItSystemUsage(usage.Id);
                Assert.Empty(notUpdatedUsage.SensitiveDataLevels);
            }
        }




    }
}
