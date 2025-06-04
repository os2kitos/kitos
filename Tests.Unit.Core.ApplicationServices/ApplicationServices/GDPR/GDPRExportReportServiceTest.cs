using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.SystemUsage.GDPR;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Shared;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.SystemUsage;

using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GDPR
{
    public class GDPRExportReportServiceTest : WithAutoFixture
    {
        private readonly GDPRExportService _sut;
        private readonly Mock<IItSystemUsageRepository> _usageRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemUsageAttachedOptionRepository> _attachedOptionRepository;
        private readonly Mock<ISensitivePersonalDataTypeRepository> _sensitivePersonalDataTypeRepository;
        private readonly Mock<IEntityIdentityResolver> _entityIdentityResolver;

        private const string datahandlerContractTypeName = "Databehandleraftale";
        private const int datahandlerContractTypeId = 5;

        public GDPRExportReportServiceTest()
        {
            _usageRepository = new Mock<IItSystemUsageRepository>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _attachedOptionRepository = new Mock<IItSystemUsageAttachedOptionRepository>();
            _sensitivePersonalDataTypeRepository = new Mock<ISensitivePersonalDataTypeRepository>();
            _entityIdentityResolver = new Mock<IEntityIdentityResolver>();
            _sut = new GDPRExportService(_usageRepository.Object,
                _authorizationContext.Object,
                _attachedOptionRepository.Object,
                _sensitivePersonalDataTypeRepository.Object,
                _entityIdentityResolver.Object);
        }

        [Fact]
        public void GetGDPRData_Returns_Forbidden_If_User_Does_Not_Have_Read_Access_In_Organization()
        {
            //Arrange
            var orgId = Math.Abs(A<int>());

            //Act
            var result = _sut.GetGDPRData(orgId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GetGDPRData_Returns_List_Of_GDPR_Data_From_System_Usages_In_Organization()
        {
            //Arrange
            var orgId = Math.Abs(A<int>());
            var system = CreateItSystem(orgId);
            var contract = CreateItContract(datahandlerContractTypeId, datahandlerContractTypeName);
            var usage = CreateSystemUsage(system, contract, orgId);
            usage.AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>
            {
                new DataProcessingRegistration {IsAgreementConcluded = YesNoIrrelevantOption.YES, Name = A<string>(), InsecureCountriesSubjectToDataTransfer = {CreateCountryOption(), CreateCountryOption(), CreateCountryOption()}},
                new DataProcessingRegistration {IsAgreementConcluded = YesNoIrrelevantOption.IRRELEVANT, Name = A<string>(), InsecureCountriesSubjectToDataTransfer = {CreateCountryOption(), CreateCountryOption()}}
            };

            var sensitivePersonalDataType = CreateSensitivePersonalDataType();

            var attachedOption = CreateAttachedOption(usage.Id, sensitivePersonalDataType.Id);


            var system2 = CreateItSystem(orgId);
            var usage2 = CreateSystemUsage(system2, null, orgId);

            IQueryable<ItSystemUsage> itSystemUsages = new List<ItSystemUsage>()
            {
                usage,
                usage2
            }.AsQueryable();

            IEnumerable<AttachedOption> attachedOptions = new List<AttachedOption>()
            {
                attachedOption
            };

            IEnumerable<SensitivePersonalDataType> sensitivePersonalDataTypes = new List<SensitivePersonalDataType>()
            {
                sensitivePersonalDataType
            };

            _usageRepository.Setup(x => x.GetSystemUsagesFromOrganization(orgId))
                .Returns(itSystemUsages);
            _attachedOptionRepository.Setup(x => x.GetBySystemUsageId(usage.Id))
                .Returns(attachedOptions);
            _attachedOptionRepository.Setup(x => x.GetBySystemUsageId(usage2.Id))
                .Returns(new List<AttachedOption>());
            _sensitivePersonalDataTypeRepository.Setup(x => x.GetSensitivePersonalDataTypes())
                .Returns(sensitivePersonalDataTypes);
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId))
                .Returns(OrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.GetGDPRData(orgId);

            //Assert
            Assert.True(result.Ok);
            var gdprExportReports = result.Value;
            var gdprExportReport1 = gdprExportReports.First();
            var gdprExportReport2 = gdprExportReports.Last();

            AssertGdprExportReportExtracted(usage, gdprExportReport1);
            AssertGdprExportReportExtracted(usage2, gdprExportReport2);
            AssertSensitivePersonalDataType(sensitivePersonalDataType, gdprExportReport1);
        }

        [Fact]
        public void GetGDPRData_Returns_List_Of_GDPR_Data_From_System_Usages_In_Organization_With_Contract_With_No_ContractType()
        {
            //Arrange
            var orgId = Math.Abs(A<int>());
            var system = CreateItSystem(orgId);
            var contract = CreateItContract(null, null);
            var usage = CreateSystemUsage(system, contract, orgId);

            var sensitivePersonalDataType = CreateSensitivePersonalDataType();

            var attachedOption = CreateAttachedOption(usage.Id, sensitivePersonalDataType.Id);


            var system2 = CreateItSystem(orgId);
            var usage2 = CreateSystemUsage(system2, null, orgId);

            IQueryable<ItSystemUsage> itSystemUsages = new List<ItSystemUsage>()
            {
                usage,
                usage2
            }.AsQueryable();

            IEnumerable<AttachedOption> attachedOptions = new List<AttachedOption>()
            {
                attachedOption
            };

            IEnumerable<SensitivePersonalDataType> sensitivePersonalDataTypes = new List<SensitivePersonalDataType>()
            {
                sensitivePersonalDataType
            };

            _usageRepository.Setup(x => x.GetSystemUsagesFromOrganization(orgId))
                .Returns(itSystemUsages);
            _attachedOptionRepository.Setup(x => x.GetBySystemUsageId(usage.Id))
                .Returns(attachedOptions);
            _attachedOptionRepository.Setup(x => x.GetBySystemUsageId(usage2.Id))
                .Returns(new List<AttachedOption>());
            _sensitivePersonalDataTypeRepository.Setup(x => x.GetSensitivePersonalDataTypes())
                .Returns(sensitivePersonalDataTypes);
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId))
                .Returns(OrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.GetGDPRData(orgId);

            //Assert
            Assert.True(result.Ok);
            var gdprExportReports = result.Value;
            var gdprExportReport1 = gdprExportReports.First();
            var gdprExportReport2 = gdprExportReports.Last();

            AssertGdprExportReportExtracted(usage, gdprExportReport1);
            AssertGdprExportReportExtracted(usage2, gdprExportReport2);
            AssertSensitivePersonalDataType(sensitivePersonalDataType, gdprExportReport1);
        }

        private void AssertSensitivePersonalDataType(SensitivePersonalDataType expected, GDPRExportReport actual)
        {
            if (actual.SensitiveData)
            {
                Assert.Equal(expected.Name, actual.SensitiveDataTypes.First());
            }
            else
            {
                Assert.Empty(actual.SensitiveDataTypes);
            }
        }

        private void AssertGdprExportReportExtracted(ItSystemUsage usage, GDPRExportReport gdprExportReport)
        {
            Assert.Equal(usage.ItSystem.Uuid.ToString("D"), gdprExportReport.SystemUuid);
            Assert.Equal(usage.ItSystem.Name, gdprExportReport.SystemName);
            Assert.Equal(usage.isBusinessCritical, gdprExportReport.BusinessCritical);
            Assert.Equal(usage.DPIA, gdprExportReport.DPIA);
            Assert.Equal(usage.DPIADateFor, gdprExportReport.DPIADate);

            Assert.Equal(usage.riskAssessment, gdprExportReport.RiskAssessment);
            Assert.Equal(usage.riskAssesmentDate, gdprExportReport.RiskAssessmentDate);
            Assert.Equal(usage.preriskAssessment, gdprExportReport.PreRiskAssessment);
            Assert.Equal(usage.PlannedRiskAssessmentDate, gdprExportReport.PlannedRiskAssessmentDate);
            Assert.Equal(usage.noteRisks, gdprExportReport.RiskAssessmentNotes);

            Assert.Equal(usage.TechnicalSupervisionDocumentationUrl, gdprExportReport.TechnicalSupervisionDocumentationUrl);
            Assert.Equal(usage.TechnicalSupervisionDocumentationUrlName,
                gdprExportReport.TechnicalSupervisionDocumentationUrlName);
            Assert.Equal(usage.UserSupervisionDocumentationUrl, usage.UserSupervisionDocumentationUrl);
            Assert.Equal(usage.UserSupervisionDocumentationUrlName, gdprExportReport.UserSupervisionDocumentationUrlName);
            Assert.Equal(usage.DPIAdeleteDate, gdprExportReport.NextDataRetentionEvaluationDate);

            AssertCountriesSubjectToTransferMatches(usage, gdprExportReport);

            if (!string.IsNullOrEmpty(usage.LinkToDirectoryUrl))
            {
                Assert.True(gdprExportReport.LinkToDirectory);
            }
            else
            {
                Assert.False(gdprExportReport.LinkToDirectory);
            }

            Assert.Equal(usage.AssociatedDataProcessingRegistrations?.Any(x => x.IsAgreementConcluded == YesNoIrrelevantOption.YES) == true, gdprExportReport.DataProcessingAgreementConcluded);

            if (usage.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.NONE))
            {
                Assert.True(gdprExportReport.NoData);
            }
            else
            {
                Assert.False(gdprExportReport.NoData);
            }

            if (usage.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.PERSONALDATA))
            {
                Assert.True(gdprExportReport.PersonalData);
            }
            else
            {
                Assert.False(gdprExportReport.PersonalData);
            }

            if (usage.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.SENSITIVEDATA))
            {
                Assert.True(gdprExportReport.SensitiveData);
            }
            else
            {
                Assert.False(gdprExportReport.SensitiveData);
            }

            if (usage.SensitiveDataLevels.Any(x => x.SensitivityDataLevel == SensitiveDataLevel.LEGALDATA))
            {
                Assert.True(gdprExportReport.LegalData);
            }
            else
            {
                Assert.False(gdprExportReport.LegalData);
            }

        }

        private void AssertCountriesSubjectToTransferMatches(ItSystemUsage usage, GDPRExportReport report)
        {
            var countriesInReport = report.InsecureCountriesSubjectToDataTransfer;
            foreach (var dpr in usage.AssociatedDataProcessingRegistrations)
            {
                Assert.True(dpr.InsecureCountriesSubjectToDataTransfer.All(country => countriesInReport.Contains(country.Name)));
            }
        }

        private SensitivePersonalDataType CreateSensitivePersonalDataType()
        {
            return new SensitivePersonalDataType()
            {
                Id = Math.Abs(A<int>()),
                Name = A<string>()
            };
        }

        private DataProcessingCountryOption CreateCountryOption()
        {
            return new DataProcessingCountryOption
            {
                Name = A<string>()
            };
        }

        private AttachedOption CreateAttachedOption(int usageId, int sensitiveDataTypeId)
        {
            return new AttachedOption()
            {
                OptionId = sensitiveDataTypeId,
                ObjectType = EntityType.ITSYSTEMUSAGE,
                ObjectId = usageId,
                OptionType = OptionType.SENSITIVEPERSONALDATA
            };
        }

        private ItContract CreateItContract(int? contractTypeId, string contractTypeName)
        {
            if (contractTypeId == null)
            {
                return new ItContract();
            }
            return new ItContract()
            {
                ContractType = new ItContractType()
                {
                    Id = contractTypeId.Value,
                    Name = contractTypeName
                }
            };
        }

        private ItSystem CreateItSystem(int orgId)
        {
            return new ItSystem()
            {

                Name = A<string>(),
                OrganizationId = orgId,
                AccessModifier = AccessModifier.Public
            };
        }

        private ItSystemUsage CreateSystemUsage(ItSystem system, ItContract contract, int orgId)
        {
            var usage = new ItSystemUsage()
            {
                Id = Math.Abs(A<int>()),
                ItSystem = system,
                Contracts = contract != null ? new List<ItContractItSystemUsage>()
                {
                    new()
                    {
                        ItContract = contract
                    },
                } : new List<ItContractItSystemUsage>(),
                OrganizationId = orgId,
                isBusinessCritical = A<DataOptions>(),
                noteRisks = A<string>(),
                LinkToDirectoryUrl = A<string>(),
                HostedAt = A<HostedAt>(),
                SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel>()
                {
                    new()
                    {
                        SensitivityDataLevel = A<SensitiveDataLevel>()
                    }
                },
            };
            usage.UpdateRetentionPeriodDefined(DataOptions.YES);
            usage.UpdateNextDataRetentionEvaluationDate(A<DateTime>());
            usage.UpdateDPIAConducted(DataOptions.YES);
            usage.UpdateDPIADate(A<DateTime>());
            usage.UpdateUserSupervisionDocumentation(A<string>(), A<string>());
            usage.UpdateTechnicalPrecautionsDocumentation(A<string>(), A<string>());
            usage.UpdateRiskAssessment(DataOptions.YES);
            usage.UpdateRiskAssessmentDate(A<DateTime>());
            usage.UpdatePlannedRiskAssessmentDate(A<DateTime>());
            usage.UpdateRiskAssessmentLevel(A<RiskLevel>());
            return usage;
        }
    }
}
