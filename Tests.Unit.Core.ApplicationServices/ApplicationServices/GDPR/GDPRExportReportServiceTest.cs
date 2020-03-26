using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.SystemUsage.GDPR;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
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
        private readonly Mock<IAttachedOptionRepository> _attachedOptionRepository;
        private readonly Mock<ISensitivePersonalDataTypeRepository> _sensitivePersonalDataTypeRepository;

        public GDPRExportReportServiceTest()
        {
            _usageRepository = new Mock<IItSystemUsageRepository>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _attachedOptionRepository = new Mock<IAttachedOptionRepository>();
            _sensitivePersonalDataTypeRepository = new Mock<ISensitivePersonalDataTypeRepository>();
            _sut = new GDPRExportService(_usageRepository.Object,
                _authorizationContext.Object,
                _attachedOptionRepository.Object,
                _sensitivePersonalDataTypeRepository.Object);
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
            var contract = CreateItContract("Databehandleraftale");
            var usage = CreateSystemUsage(system, contract, orgId);

            var sensitivePersonalDataType = CreateSensitivePersonalDataType();

            var attachedOption = CreateAttachedOption(usage.Id, sensitivePersonalDataType.Id);


            var system2 = CreateItSystem(orgId);
            var usage2 = CreateSystemUsage(system2, null, orgId);

            IEnumerable<ItSystemUsage> itSystemUsages = new List<ItSystemUsage>()
            {
                usage,
                usage2
            };

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
            _attachedOptionRepository.Setup(x => x.GetAttachedOptions())
                .Returns(attachedOptions);
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
            Assert.Equal(sensitivePersonalDataType.Name, gdprExportReport1.SensitiveDataTypes.First());
        }

        private void AssertGdprExportReportExtracted(ItSystemUsage usage, GDPRExportReport gdprExportReport)
        {
            Assert.Equal(usage.ItSystem.Name, gdprExportReport.SystemName);
            Assert.Equal(usage.isBusinessCritical, gdprExportReport.BusinessCritical);
            Assert.Equal(usage.dataProcessorControl, gdprExportReport.DataProcessorControl);
            Assert.Equal(usage.DPIA, gdprExportReport.DPIA);
            Assert.Equal(usage.riskAssessment, gdprExportReport.RiskAssessment);
            Assert.Equal(usage.preriskAssessment, gdprExportReport.PreRiskAssessment);

            if (! string.IsNullOrEmpty(usage.LinkToDirectoryUrl))
            {
                Assert.True(gdprExportReport.LinkToDirectory);
            }
            else
            {
                Assert.False(gdprExportReport.LinkToDirectory);
            }

            if (usage.Contracts.Any(x => x.ItContract.ContractType.Name == "Databehandleraftale"))
            {
                Assert.True(gdprExportReport.DataProcessorContract);
            }
            else
            {
                Assert.False(gdprExportReport.DataProcessorContract);
            }

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

        private SensitivePersonalDataType CreateSensitivePersonalDataType()
        {
            return new SensitivePersonalDataType()
            {
                Id = Math.Abs(A<int>()),
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

        private ItContract CreateItContract(string contractTypeName)
        {
            return new ItContract()
            {
                ContractType = new ItContractType()
                {
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
            return new ItSystemUsage()
            {
                Id = Math.Abs(A<int>()),
                ItSystem = system,
                Contracts = contract != null ? new List<ItContractItSystemUsage>()
                {
                    new ItContractItSystemUsage()
                    {
                        ItContract = contract
                    },
                } : new List<ItContractItSystemUsage>(),
                OrganizationId = orgId,
                isBusinessCritical = A<DataOptions>(),
                dataProcessorControl = A<DataOptions>(),
                DPIA = A<DataOptions>(),
                riskAssessment = A<DataOptions>(),
                LinkToDirectoryUrl = A<string>(),
                HostedAt = A<HostedAt>(),
                preriskAssessment = A<RiskLevel>(),
                SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel>()
                {
                    new ItSystemUsageSensitiveDataLevel()
                    {
                        SensitivityDataLevel = A<SensitiveDataLevel>()
                    }
                }
            };
        }
    }
}
