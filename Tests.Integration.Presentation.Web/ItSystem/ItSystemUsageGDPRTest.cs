using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using CsvHelper.Configuration.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;
using Xunit.Sdk;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class ItSystemUsageGDPRTest : WithAutoFixture
    {


        [Fact]
        public async Task Can_Change_HostedAtOptions()
        {
            //Arrange
            var hostedAtOption = A<HostedAt>();
            var body = new { HostedAt = hostedAtOption };
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, system.OrganizationId);

            //Act
            var itSystemUsageDTO = await ItSystemUsageHelper.PatchSystemUsage(usage.Id, organizationId, body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.HostedAt);
            Assert.Equal(hostedAtOption, itSystemUsageDTO.HostedAt.Value);

        }



        [Fact]
        public async Task Can_Change_IsBusinessCritical()
        {
            //Arrange
            var dataOption = A<DataOptions>();
            var body = new { IsBusinessCritical = dataOption };

            //Act
            var itSystemUsageDTO = await Create_System_Usage_And_Change_Value_By_Body(body);

            //Assert
            Assert.NotNull(itSystemUsageDTO.IsBusinessCritical);
            Assert.Equal(dataOption, itSystemUsageDTO.IsBusinessCritical.Value);
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

        [Fact]
        public async Task Can_Get_GDPRExportReport_With_All_Fields_Set()
        {
            //Arrange
            var sensitiveDataLevel = A<SensitiveDataLevel>();
            var datahandlerContractTypeId = "5";
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);
            var dataProcessingRegistrationDto = await DataProcessingRegistrationHelper.CreateAsync(organizationId, A<string>());
            await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dataProcessingRegistrationDto.Id, YesNoIrrelevantOption.YES);
            using var setSystemResponse = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dataProcessingRegistrationDto.Id, usage.Id);
            Assert.Equal(HttpStatusCode.OK, setSystemResponse.StatusCode);
            var body = new
            {
                HostedAt = A<HostedAt>(),
                IsBusinessCritical = A<DataOptions>(),
                DataProcessorControl = A<DataOptions>(),
                RiskAssessment = A<DataOptions>(),
                PreRiskAssessment = A<RiskLevel>(),
                DPIA = A<DataOptions>()

            };
            var contract = await ItContractHelper.CreateContract(A<string>(), organizationId);
            await ItContractHelper.PatchContract(contract.Id, organizationId, new { contractTypeId = datahandlerContractTypeId });
            await ItContractHelper.AddItSystemUsage(contract.Id, usage.Id, organizationId);
            await ItSystemUsageHelper.PatchSystemUsage(usage.Id, organizationId, body);
            await ItSystemUsageHelper.AddSensitiveDataLevel(usage.Id, sensitiveDataLevel);

            var expectedUsage = await ItSystemHelper.GetItSystemUsage(usage.Id);

            //Act
            var report = await ItSystemUsageHelper.GetGDPRExportReport(organizationId);

            //Assert
            var gdprExportReport = Assert.Single(report.Where(x => x.Name == system.Name));
            AssertCorrectGdprExportReport(expectedUsage, gdprExportReport, true);
            AssertSensitiveDataLevel(sensitiveDataLevel, gdprExportReport);

        }

        [Fact]
        public async Task Can_Get_GDPRExportReport_With_Fresh_Usage()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), organizationId, AccessModifier.Public);
            var usage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var expectedUsage = await ItSystemHelper.GetItSystemUsage(usage.Id);

            //Act
            var report = await ItSystemUsageHelper.GetGDPRExportReport(organizationId);

            //Assert
            var gdprExportReport = Assert.Single(report.Where(x => x.Name == system.Name));
            AssertCorrectGdprExportReport(expectedUsage, gdprExportReport, false);
            AssertEmptyString(gdprExportReport.SensitiveDataTypes);
        }


        private void AssertCorrectGdprExportReport(ItSystemUsageDTO expected, GdprExportReportCsvFormat actual, bool hasConcludedDataProcessingAgreement)
        {
            AssertDataOption(expected.IsBusinessCritical, actual.BusinessCritical);
            AssertDataOption(expected.RiskAssessment, actual.RiskAssessment);
            AssertDataOption(expected.DPIA, actual.DPIA);
            AssertRiskLevel(expected.PreRiskAssessment, actual.PreRiskAssessment);
            AssertHostedAt(expected.HostedAt, actual.HostedAt);
            if (hasConcludedDataProcessingAgreement)
            {
                AssertYes(actual.Datahandler);
            }
            else
            {
                AssertNo(actual.Datahandler);
            }
        }

        private void AssertSensitiveDataLevel(SensitiveDataLevel expected, GdprExportReportCsvFormat actual)
        {
            switch (expected)
            {
                case SensitiveDataLevel.NONE:
                    AssertYes(actual.NoData);
                    AssertNo(actual.PersonalData);
                    AssertNo(actual.SensitiveData);
                    AssertNo(actual.LegalData);
                    return;
                case SensitiveDataLevel.PERSONALDATA:
                    AssertNo(actual.NoData);
                    AssertYes(actual.PersonalData);
                    AssertNo(actual.SensitiveData);
                    AssertNo(actual.LegalData);
                    return;
                case SensitiveDataLevel.SENSITIVEDATA:
                    AssertNo(actual.NoData);
                    AssertNo(actual.PersonalData);
                    AssertYes(actual.SensitiveData);
                    AssertNo(actual.LegalData);
                    return;
                case SensitiveDataLevel.LEGALDATA:
                    AssertNo(actual.NoData);
                    AssertNo(actual.PersonalData);
                    AssertNo(actual.SensitiveData);
                    AssertYes(actual.LegalData);
                    return;
                default:
                    throw new AssertActualExpectedException(expected, actual, "Expected is not a correct SensitiveDataLevel");

            }
        }

        private void AssertHostedAt(HostedAt? expected, string actual)
        {
            if (expected == null)
            {
                AssertEmptyString(actual);
                return;
            }
            switch (expected)
            {
                case HostedAt.UNDECIDED:
                    AssertEmptyString(actual);
                    return;
                case HostedAt.ONPREMISE:
                    Assert.Equal("On-premise", actual);
                    return;
                case HostedAt.EXTERNAL:
                    Assert.Equal("Eksternt", actual);
                    return;
                case null:
                    break;
                default:
                    throw new AssertActualExpectedException(expected, actual, "Expected is not a correct HostedAt");
            }
        }

        private void AssertRiskLevel(RiskLevel? expected, string actual)
        {
            if (expected == null)
            {
                AssertEmptyString(actual);
                return;
            }
            switch (expected)
            {
                case RiskLevel.LOW:
                    Assert.Equal("Lav", actual);
                    return;
                case RiskLevel.MIDDLE:
                    Assert.Equal("Middel", actual);
                    return;
                case RiskLevel.HIGH:
                    Assert.Equal("Høj", actual);
                    return;
                case RiskLevel.UNDECIDED:
                    AssertEmptyString(actual);
                    return;
                case null:
                    break;
                default:
                    throw new AssertActualExpectedException(expected, actual, "Expected is not a correct RiskLevel");
            }
        }

        private void AssertDataOption(DataOptions? expected, string actual)
        {
            if (expected == null)
            {
                AssertEmptyString(actual);
                return;
            }
            switch (expected)
            {
                case DataOptions.NO:
                    AssertNo(actual);
                    return;
                case DataOptions.YES:
                    AssertYes(actual);
                    return;
                case DataOptions.DONTKNOW:
                    Assert.Equal("Ved ikke", actual);
                    return;
                case DataOptions.UNDECIDED:
                    AssertEmptyString(actual);
                    return;
                default:
                    throw new AssertActualExpectedException(expected, actual, "Expected is not a correct DataOption");
            }
        }

        private void AssertYes(string actual)
        {
            Assert.Equal("Ja", actual);
        }

        private void AssertNo(string actual)
        {
            Assert.Equal("Nej", actual);
        }

        private void AssertEmptyString(string actual)
        {
            Assert.Equal("", actual);
        }

    }

    public class GdprExportReportCsvFormat
    {
        [Name("Navn")]
        public string Name { get; set; }
        [Name("Ingen persondata")]
        public string NoData { get; set; }
        [Name("Almindelige persondata")]
        public string PersonalData { get; set; }
        [Name("Følsomme persondata")]
        public string SensitiveData { get; set; }
        [Name("Straffesager og lovovertrædelser")]
        public string LegalData { get; set; }
        [Name("Valgte følsomme persondata")]
        public string SensitiveDataTypes { get; set; }
        [Name("Forretningskritisk IT-System")]
        public string BusinessCritical { get; set; }
        [Name("Databehandleraftale")]
        public string Datahandler { get; set; }
        [Name("Link til fortegnelse")]
        public string DirectoryUrl { get; set; }
        [Name("Foretaget risikovurdering")]
        public string RiskAssessment { get; set; }
        [Name("Hvad viste seneste risikovurdering")]
        public string PreRiskAssessment { get; set; }
        [Name("Gennemført DPIA / Konsekvensanalyse")]
        public string DPIA { get; set; }
        [Name("IT-Systemet driftes")]
        public string HostedAt { get; set; }
    }
}
