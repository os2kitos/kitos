using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Repositories.GDPR;
using Moq;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemUsageResponseMapperTest : WithAutoFixture
    {
        private readonly ItSystemUsageResponseMapper _sut;
        private readonly Mock<IItSystemUsageAttachedOptionRepository> _attachedOptionsRepositoryMock;
        private readonly Mock<ISensitivePersonalDataTypeRepository> _sensitivePersonalDataTypeRepositoryMock;
        private readonly Mock<IGenericRepository<RegisterType>> _registerTypeRepositoryMock;
        private readonly Mock<IExternalReferenceResponseMapper> _externalReferenceResponseMapperMock;

        public ItSystemUsageResponseMapperTest()
        {
            _attachedOptionsRepositoryMock = new Mock<IItSystemUsageAttachedOptionRepository>();
            _sensitivePersonalDataTypeRepositoryMock = new Mock<ISensitivePersonalDataTypeRepository>();
            _registerTypeRepositoryMock = new Mock<IGenericRepository<RegisterType>>();
            _externalReferenceResponseMapperMock = new Mock<IExternalReferenceResponseMapper>();
            _sut = new ItSystemUsageResponseMapper(
                _attachedOptionsRepositoryMock.Object,
                _sensitivePersonalDataTypeRepositoryMock.Object,
                _registerTypeRepositoryMock.Object,
                _externalReferenceResponseMapperMock.Object
                );
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_Root_Properties()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(itSystemUsage.Uuid, dto.Uuid);
            Assert.Equal(itSystemUsage.LastChanged, dto.LastModified);
            AssertUser(itSystemUsage.ObjectOwner, dto.CreatedBy);
            AssertUser(itSystemUsage.LastChangedByUser, dto.LastModifiedBy);
            AssertIdentity(itSystemUsage.ItSystem, dto.SystemContext);
            AssertOrganization(itSystemUsage.Organization, dto.OrganizationContext);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_General_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignGeneralPropertiesSection(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(itSystemUsage.LocalSystemId, dto.General.LocalSystemId);
            Assert.Equal(itSystemUsage.LocalCallName, dto.General.LocalCallName);
            Assert.Equal(itSystemUsage.Note, dto.General.Notes);
            AssertUserCount(itSystemUsage, dto.General.NumberOfExpectedUsers);
            Assert.Equal(itSystemUsage.Version, dto.General.SystemVersion);
            AssertIdentity(itSystemUsage.MainContract.ItContract, dto.General.MainContract);
            Assert.Equal(itSystemUsage.Concluded, dto.General.Validity.ValidFrom);
            Assert.Equal(itSystemUsage.ExpirationDate, dto.General.Validity.ValidTo);
            Assert.Equal(itSystemUsage.LifeCycleStatus, dto.General.Validity.LifeCycleStatus?.ToLifeCycleStatusType());
            Assert.Equal(itSystemUsage.IsActiveAccordingToDateFields, dto.General.Validity.ValidAccordingToValidityPeriod);
            Assert.Equal(itSystemUsage.IsActiveAccordingToLifeCycle, dto.General.Validity.ValidAccordingToLifeCycle);
            Assert.Equal(itSystemUsage.IsActiveAccordingToMainContract, dto.General.Validity.ValidAccordingToMainContract);
            Assert.Equal(itSystemUsage.CheckSystemValidity().Result, dto.General.Validity.Valid);
            Assert.Equal(itSystemUsage.WebAccessibilityCompliance, dto.General.WebAccessibilityCompliance?.ToYesNoPartiallyOption());
            Assert.Equal(itSystemUsage.LastWebAccessibilityCheck, dto.General.LastWebAccessibilityCheck);
            Assert.Equal(itSystemUsage.WebAccessibilityNotes, dto.General.WebAccessibilityNotes);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_Role_Assignment_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignRoles(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            var expected = itSystemUsage.Rights.Select(right => new
            {
                roleId = right.Role.Uuid,
                roleName = right.Role.Name,
                userId = right.User.Uuid,
                userName = right.User.GetFullName()
            }).ToList();
            var actual = dto.Roles.Select(roleAssignment => new
            {
                roleId = roleAssignment.Role.Uuid,
                roleName = roleAssignment.Role.Name,
                userId = roleAssignment.User.Uuid,
                userName = roleAssignment.User.Name
            }).ToList();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var comparison in expected.Zip(actual, (expectedEntry, actualEntry) => new { expectedEntry, actualEntry }).ToList())
            {
                Assert.Equal(comparison.expectedEntry.roleId, comparison.actualEntry.roleId);
                Assert.Equal(comparison.expectedEntry.roleName, comparison.actualEntry.roleName);
                Assert.Equal(comparison.expectedEntry.userId, comparison.actualEntry.userId);
                Assert.Equal(comparison.expectedEntry.userName, comparison.actualEntry.userName);
            }
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_LocalKLEDeviations_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignKle(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            var expectedAdditions = itSystemUsage.TaskRefs.Select(tr => (tr.TaskKey, tr.Uuid)).ToList();
            var actualAdditions = dto.LocalKLEDeviations.AddedKLE.Select(kle => (kle.Name, kle.Uuid)).ToList();
            Assert.Equal(expectedAdditions, actualAdditions);

            var expectedRemovals = itSystemUsage.TaskRefsOptOut.Select(tr => (tr.TaskKey, tr.Uuid)).ToList();
            var actualRemovals = dto.LocalKLEDeviations.RemovedKLE.Select(kle => (kle.Name, kle.Uuid)).ToList();
            Assert.Equal(expectedRemovals, actualRemovals);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_OrganizationalUsage_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignOrganizationalUsage(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            AssertIdentity(itSystemUsage.ResponsibleUsage.OrganizationUnit, dto.OrganizationUsage.ResponsibleOrganizationUnit);
            var expectedUnits = itSystemUsage.UsedBy.Select(x => x.OrganizationUnit).OrderBy(x => x.Name).ToList();
            var actualUnits = dto.OrganizationUsage.UsingOrganizationUnits.OrderBy(x => x.Name).ToList();
            Assert.Equal(expectedUnits.Count, actualUnits.Count);
            foreach (var comparison in expectedUnits.Zip(actualUnits, (expected, actual) => new { expected, actual }).ToList())
            {
                AssertIdentity(comparison.expected, comparison.actual);
            }
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_ExternalReferences_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignExternalReferences(itSystemUsage);

            var mappedReferences = Many<ExternalReferenceDataResponseDTO>();
            _externalReferenceResponseMapperMock
                .Setup(x => x.MapExternalReferences(itSystemUsage.ExternalReferences))
                .Returns(mappedReferences);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            _externalReferenceResponseMapperMock.Verify(x => x.MapExternalReferences(itSystemUsage.ExternalReferences), Times.Once);
            Assert.Equivalent(mappedReferences, dto.ExternalReferences);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_OutgoingSystemRelations_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignOutgoingSystemRelations(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            var expectedRelations = itSystemUsage.UsageRelations.ToList();
            var actualRelations = dto.OutgoingSystemRelations.ToList();
            Assert.Equal(expectedRelations.Count, actualRelations.Count);
            foreach (var comparison in expectedRelations.OrderBy(x => x.Uuid).Zip(actualRelations.OrderBy(x => x.Uuid), (expected, actual) => new { expected, actual }).ToList())
            {
                AssertOutgoingRelation(comparison.expected, comparison.actual);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSystemUsageDTO_Maps_Archiving_Properties_Section(bool withCrossReferences)
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignArchiving(itSystemUsage, withCrossReferences);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(itSystemUsage.ArchiveFromSystem, dto.Archiving.Active);
            Assert.Equal(itSystemUsage.ArchiveNotes, dto.Archiving.Notes);
            Assert.Equal(itSystemUsage.Registertype, dto.Archiving.DocumentBearing);
            Assert.Equal(itSystemUsage.ArchiveFreq, dto.Archiving.FrequencyInMonths);
            AssertOptionalIdentity(itSystemUsage.ArchiveLocation, dto.Archiving.Location);
            AssertOptionalIdentity(itSystemUsage.ArchiveTestLocation, dto.Archiving.TestLocation);
            AssertOptionalIdentity(itSystemUsage.ArchiveType, dto.Archiving.Type);
            AssertOptionalIdentity(itSystemUsage.ArchiveSupplier, dto.Archiving.Supplier);
            var expectedArchivePeriods = itSystemUsage.ArchivePeriods.OrderBy(x => x.UniqueArchiveId).ToList();
            var actualJournalPeriods = dto.Archiving.JournalPeriods.OrderBy(x => x.ArchiveId).ToList();
            Assert.Equal(expectedArchivePeriods.Count, actualJournalPeriods.Count);
            foreach (var comparison in expectedArchivePeriods.Zip(actualJournalPeriods, (expected, actual) => new { expected, actual }).ToList())
            {
                AssertArchivePeriod(comparison.expected, comparison.actual);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSystemUsageDTO_Maps_GDPR_Properties_Section(bool withCrossReferences)
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            var (expectedSensitivePersonData, expectedRegisterTypes) = AssignGDPR(itSystemUsage, withCrossReferences);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(dto.GDPR.Purpose, itSystemUsage.GeneralPurpose);
            AssertYesNoExtended(dto.GDPR.BusinessCritical, itSystemUsage.isBusinessCritical);
            AssertYesNoExtended(dto.GDPR.DPIAConducted, itSystemUsage.DPIA);
            AssertSimpleLink(dto.GDPR.DPIADocumentation, itSystemUsage.DPIASupervisionDocumentationUrlName, itSystemUsage.DPIASupervisionDocumentationUrl);
            AssertHostedAt(dto.GDPR.HostedAt, itSystemUsage.HostedAt);
            AssertSimpleLink(dto.GDPR.DirectoryDocumentation, itSystemUsage.LinkToDirectoryUrlName, itSystemUsage.LinkToDirectoryUrl);
            Assert.Equal(dto.GDPR.DataSensitivityLevels.Select(MapDataSensitivity).OrderBy(x => x).ToList(), itSystemUsage.SensitiveDataLevels.Select(x => x.SensitivityDataLevel).OrderBy(x => x).ToList());
            AssertYesNoExtended(dto.GDPR.TechnicalPrecautionsInPlace, itSystemUsage.precautions);
            AssertAppliedPrecautions(dto.GDPR.TechnicalPrecautionsApplied, itSystemUsage);
            AssertSimpleLink(dto.GDPR.TechnicalPrecautionsDocumentation, itSystemUsage.TechnicalSupervisionDocumentationUrlName, itSystemUsage.TechnicalSupervisionDocumentationUrl);
            AssertYesNoExtended(dto.GDPR.DPIAConducted, itSystemUsage.DPIA);
            Assert.Equal(dto.GDPR.DPIADate, itSystemUsage.DPIADateFor);
            AssertYesNoExtended(dto.GDPR.RetentionPeriodDefined, itSystemUsage.answeringDataDPIA);
            Assert.Equal(dto.GDPR.DataRetentionEvaluationFrequencyInMonths, itSystemUsage.numberDPIA);
            Assert.Equal(dto.GDPR.NextDataRetentionEvaluationDate, itSystemUsage.DPIAdeleteDate);
            AssertYesNoExtended(dto.GDPR.UserSupervision, itSystemUsage.UserSupervision);
            Assert.Equal(dto.GDPR.UserSupervisionDate, itSystemUsage.UserSupervisionDate);
            AssertSimpleLink(dto.GDPR.UserSupervisionDocumentation, itSystemUsage.UserSupervisionDocumentationUrlName, itSystemUsage.UserSupervisionDocumentationUrl);
            AssertYesNoExtended(dto.GDPR.RiskAssessmentConducted, itSystemUsage.riskAssessment);
            Assert.Equal(dto.GDPR.RiskAssessmentConductedDate, itSystemUsage.riskAssesmentDate);
            AssertSimpleLink(dto.GDPR.RiskAssessmentDocumentation, itSystemUsage.RiskSupervisionDocumentationUrlName, itSystemUsage.RiskSupervisionDocumentationUrl);
            Assert.Equal(dto.GDPR.RiskAssessmentNotes, itSystemUsage.noteRisks);
            AssertRiskLevel(dto.GDPR.RiskAssessmentResult, itSystemUsage.preriskAssessment);
            Assert.Equal(dto.GDPR.PlannedRiskAssessmentDate, itSystemUsage.PlannedRiskAssessmentDate);

            Assert.Equal(dto.GDPR.SpecificPersonalData.Count(), itSystemUsage.PersonalDataOptions.Count);
            foreach (var dataOption in dto.GDPR.SpecificPersonalData)
            {
                Assert.Contains(dataOption, itSystemUsage.PersonalDataOptions.Select(x => x.PersonalData.ToGDPRPersonalDataChoice()));
            }

            Assert.Equal(dto.GDPR.SensitivePersonData.Count(), expectedSensitivePersonData.Count);
            Assert.Equal(dto.GDPR.RegisteredDataCategories.Count(), expectedRegisterTypes.Count);
            foreach (var comparison in expectedSensitivePersonData.OrderBy(x => x.Name).Zip(dto.GDPR.SensitivePersonData.OrderBy(x => x.Name), (expected, actual) => new { expected, actual }))
            {
                AssertIdentity(comparison.expected, comparison.actual);
            }

            foreach (var comparison in expectedRegisterTypes.OrderBy(x => x.Name).Zip(dto.GDPR.RegisteredDataCategories.OrderBy(x => x.Name), (expected, actual) => new { expected, actual }))
            {
                AssertIdentity(comparison.expected, comparison.actual);
            }
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        public void Can_Map_Incoming_System_Relation(bool withContract, bool withFrequency, bool withInterface)
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            var incomingSystemRelation = CreateIncomingSystemRelation(itSystemUsage, withContract, withFrequency, withInterface);

            //Act
            var dto = _sut.MapIncomingSystemRelationDTO(incomingSystemRelation);

            //Assert
            AssertIncomingRelation(incomingSystemRelation, dto);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        public void Can_Map_Outgoing_System_Relation(bool withContract, bool withFrequency, bool withInterface)
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            var incomingSystemRelation = CreateOutgoingSystemRelation(itSystemUsage, withContract, withFrequency, withInterface);

            //Act
            var dto = _sut.MapOutgoingSystemRelationDTO(incomingSystemRelation);

            //Assert
            AssertOutgoingRelation(incomingSystemRelation, dto);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        public void Can_Map_General_System_Relation(bool withContract, bool withFrequency, bool withInterface)
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            var outgoingSystemRelation = CreateOutgoingSystemRelation(itSystemUsage, withContract, withFrequency, withInterface);

            //Act
            var dto = _sut.MapGeneralSystemRelationDTO(outgoingSystemRelation);

            //Assert
            AssertGeneralRelation(outgoingSystemRelation, dto);
        }

        [Fact]
        public void Can_Map_JournalPeriod()
        {
            //Arrange
            var archivePeriod = CreateArchivePeriod();

            //Act
            var mappedArchivePeriod = _sut.MapJournalPeriodResponseDto(archivePeriod);

            //Assert
            AssertArchivePeriod(archivePeriod, mappedArchivePeriod);
        }


        private static void AssertRiskLevel(RiskLevelChoice? actual, RiskLevel? sourceValue)
        {
            RiskLevelChoice? expected = sourceValue switch
            {
                RiskLevel.LOW => RiskLevelChoice.Low,
                RiskLevel.MIDDLE => RiskLevelChoice.Medium,
                RiskLevel.HIGH => RiskLevelChoice.High,
                RiskLevel.UNDECIDED => RiskLevelChoice.Undecided,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(sourceValue), sourceValue, null)
            };
            Assert.Equal(expected, actual);
        }

        private static void AssertAppliedPrecautions(IEnumerable<TechnicalPrecautionChoice> actual, ItSystemUsage source)
        {
            var expectedChoices = new List<TechnicalPrecautionChoice>();
            if (source.precautionsOptionsAccessControl)
                expectedChoices.Add(TechnicalPrecautionChoice.AccessControl);
            if (source.precautionsOptionsEncryption)
                expectedChoices.Add(TechnicalPrecautionChoice.Encryption);
            if (source.precautionsOptionsLogning)
                expectedChoices.Add(TechnicalPrecautionChoice.Logging);
            if (source.precautionsOptionsPseudonomisering)
                expectedChoices.Add(TechnicalPrecautionChoice.Pseudonymization);
            Assert.Equal(expectedChoices.OrderBy(x => x).ToList(), actual.OrderBy(x => x).ToList());
        }

        private static SensitiveDataLevel MapDataSensitivity(DataSensitivityLevelChoice actual)
        {
            return actual switch
            {
                DataSensitivityLevelChoice.None => SensitiveDataLevel.NONE,
                DataSensitivityLevelChoice.PersonData => SensitiveDataLevel.PERSONALDATA,
                DataSensitivityLevelChoice.SensitiveData => SensitiveDataLevel.SENSITIVEDATA,
                DataSensitivityLevelChoice.LegalData => SensitiveDataLevel.LEGALDATA,
                _ => throw new ArgumentOutOfRangeException(nameof(actual), actual, null)
            };
        }

        private static void AssertHostedAt(HostingChoice? actual, HostedAt? sourceValue)
        {
            HostingChoice? expected = sourceValue switch
            {
                HostedAt.UNDECIDED => HostingChoice.Undecided,
                HostedAt.ONPREMISE => HostingChoice.OnPremise,
                HostedAt.EXTERNAL => HostingChoice.External,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(sourceValue), sourceValue, null)
            };
            Assert.Equal(expected, actual);
        }

        private static void AssertSimpleLink(SimpleLinkDTO actual, string expectedName, string expectedUrl)
        {
            Assert.Equal(expectedName, actual.Name);
            Assert.Equal(expectedUrl, actual.Url);
        }

        private (IReadOnlyList<SensitivePersonalDataType> sensitivePersonData, IReadOnlyList<RegisterType> registerTypeData) AssignGDPR(ItSystemUsage itSystemUsage, bool withCrossReferences)
        {
            itSystemUsage.GeneralPurpose = A<string>();
            itSystemUsage.isBusinessCritical = A<DataOptions>();
            itSystemUsage.DPIA = A<DataOptions>();
            itSystemUsage.DPIADateFor = A<DateTime>();
            itSystemUsage.DPIASupervisionDocumentationUrlName = A<string>();
            itSystemUsage.DPIASupervisionDocumentationUrl = A<string>();
            itSystemUsage.HostedAt = A<HostedAt>();
            itSystemUsage.LinkToDirectoryUrlName = A<string>();
            itSystemUsage.LinkToDirectoryUrl = A<string>();
            itSystemUsage.SensitiveDataLevels = Many<SensitiveDataLevel>().Select(sensitiveDataLevel => new ItSystemUsageSensitiveDataLevel() { SensitivityDataLevel = sensitiveDataLevel }).ToList();
            itSystemUsage.PersonalDataOptions = Many<GDPRPersonalDataOption>().Select(x => new ItSystemUsagePersonalData() { PersonalData = x }).ToList();
            itSystemUsage.precautions = A<DataOptions>();
            itSystemUsage.precautionsOptionsAccessControl = A<bool>();
            itSystemUsage.precautionsOptionsEncryption = A<bool>();
            itSystemUsage.precautionsOptionsLogning = A<bool>();
            itSystemUsage.precautionsOptionsPseudonomisering = A<bool>();
            itSystemUsage.TechnicalSupervisionDocumentationUrlName = A<string>();
            itSystemUsage.TechnicalSupervisionDocumentationUrl = A<string>();
            itSystemUsage.answeringDataDPIA = A<DataOptions>();
            itSystemUsage.DPIAdeleteDate = A<DateTime>();
            itSystemUsage.numberDPIA = A<int>();
            itSystemUsage.riskAssessment = A<DataOptions>();
            itSystemUsage.riskAssesmentDate = A<DateTime>();
            itSystemUsage.RiskSupervisionDocumentationUrlName = A<string>();
            itSystemUsage.RiskSupervisionDocumentationUrl = A<string>();
            itSystemUsage.PlannedRiskAssessmentDate = A<DateTime>();
            itSystemUsage.noteRisks = A<string>();
            itSystemUsage.preriskAssessment = A<RiskLevel>();
            itSystemUsage.UserSupervision = A<DataOptions>();
            itSystemUsage.UserSupervisionDate = A<DateTime>();
            itSystemUsage.UserSupervisionDocumentationUrlName = A<string>();
            itSystemUsage.UserSupervisionDocumentationUrl = A<string>();

            var sensitivePersonalDataTypes = Many<Guid>(10).Select(uuid => new SensitivePersonalDataType() { Id = A<int>(), Uuid = uuid, Name = A<string>() }).ToList();
            var registerTypes = Many<Guid>(10).Select(uuid => new RegisterType() { Id = A<int>(), Uuid = uuid, Name = A<string>() }).ToList();
            _sensitivePersonalDataTypeRepositoryMock.Setup(x => x.GetSensitivePersonalDataTypes()).Returns(sensitivePersonalDataTypes);
            _registerTypeRepositoryMock.Setup(x => x.Get(null, null, "")).Returns(registerTypes);
            var usedPersonalDataTypes = sensitivePersonalDataTypes
                .OrderBy(_ => A<int>())
                .Take(withCrossReferences ? sensitivePersonalDataTypes.Count / 2 : 0)
                .ToList();
            var usedSensitivePersonalDataTypesOptions = usedPersonalDataTypes
                .Select(x => new AttachedOption { Id = A<int>(), OptionType = OptionType.SENSITIVEPERSONALDATA, OptionId = x.Id });
            var usedRegisterTypes = registerTypes.OrderBy(_ => A<int>())
                .Take(withCrossReferences ? registerTypes.Count / 2 : 0)
                .ToList();
            var usedRegisterTypesOptions = usedRegisterTypes
                .Select(x => new AttachedOption { Id = A<int>(), OptionType = OptionType.REGISTERTYPEDATA, OptionId = x.Id });

            _attachedOptionsRepositoryMock.Setup(x => x.GetBySystemUsageId(itSystemUsage.Id)).Returns(usedSensitivePersonalDataTypesOptions.Concat(usedRegisterTypesOptions));

            return (usedPersonalDataTypes, usedRegisterTypes);
        }

        private void AssignArchiving(ItSystemUsage itSystemUsage, bool withOptionalCrossReferences)
        {
            itSystemUsage.ArchiveFromSystem = A<bool?>();
            itSystemUsage.ArchiveNotes = A<string>();
            itSystemUsage.Registertype = A<bool?>();
            itSystemUsage.ArchiveFreq = A<int?>();
            itSystemUsage.ArchiveLocation = withOptionalCrossReferences
                ? new ArchiveLocation { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            itSystemUsage.ArchiveTestLocation = withOptionalCrossReferences ? new ArchiveTestLocation { Uuid = A<Guid>(), Name = A<string>() } : null;
            itSystemUsage.ArchiveType = withOptionalCrossReferences ? new ArchiveType { Uuid = A<Guid>(), Name = A<string>() } : null;
            itSystemUsage.ArchiveSupplier = withOptionalCrossReferences ? new Organization() { Uuid = A<Guid>() } : null;
            itSystemUsage.ArchivePeriods = Many<string>().Select(CreateArchivePeriod).ToList();
        }

        private ArchivePeriod CreateArchivePeriod(string id = null)
        {
            return new ArchivePeriod
            {
                Approved = A<bool>(),
                StartDate = A<DateTime>(),
                EndDate = A<DateTime>(),
                UniqueArchiveId = id ?? A<string>()
            };
        }

        private static void AssertGeneralRelation(SystemRelation expected, GeneralSystemRelationResponseDTO actual)
        {
            AssertIdentity(expected.ToSystemUsage, actual.ToSystemUsage);
            AssertIdentity(expected.FromSystemUsage, actual.FromSystemUsage);
            AssertBaseRelation(expected, actual);
        }

        private static void AssertOutgoingRelation(SystemRelation expected, OutgoingSystemRelationResponseDTO actual)
        {
            AssertIdentity(expected.ToSystemUsage, actual.ToSystemUsage);
            AssertBaseRelation(expected, actual);
        }

        private static void AssertIncomingRelation(SystemRelation expected, IncomingSystemRelationResponseDTO actual)
        {
            AssertIdentity(expected.FromSystemUsage, actual.FromSystemUsage);
            AssertBaseRelation(expected, actual);
        }

        private static void AssertBaseRelation(SystemRelation expected, BaseSystemRelationResponseDTO actual)
        {
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Reference, actual.UrlReference);
            AssertOptionalIdentity(expected.AssociatedContract, actual.AssociatedContract);
            AssertOptionalIdentity(expected.UsageFrequency, actual.RelationFrequency);
            AssertOptionalIdentity(expected.RelationInterface, actual.RelationInterface);
        }

        private static void AssertOptionalIdentity<T>(T optionalExpectedIdentity, IdentityNamePairResponseDTO actualIdentity) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentity == null)
                Assert.Null(actualIdentity);
            else
                AssertIdentity(optionalExpectedIdentity, actualIdentity);
        }

        private void AssignOutgoingSystemRelations(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.UsageRelations = new[]
            {
                CreateOutgoingSystemRelation(itSystemUsage,false,false,false),
                CreateOutgoingSystemRelation(itSystemUsage,false,false,true),
                CreateOutgoingSystemRelation(itSystemUsage,false,true,true),
                CreateOutgoingSystemRelation(itSystemUsage,true,true,true)
            }.ToList();
        }

        private SystemRelation CreateOutgoingSystemRelation(ItSystemUsage itSystemUsage, bool withContract = false, bool withFrequency = false, bool withInterface = false)
        {
            return new SystemRelation(itSystemUsage)
            {
                Uuid = A<Guid>(),
                Reference = A<string>(),
                AssociatedContract = withContract ? new ItContract() { Uuid = A<Guid>(), Name = A<string>() } : null,
                RelationInterface = withInterface ? new ItInterface { Uuid = A<Guid>(), Name = A<string>() } : null,
                UsageFrequency = withFrequency ? new RelationFrequencyType() { Uuid = A<Guid>(), Name = A<string>() } : null,
                ToSystemUsage = new ItSystemUsage() { Uuid = A<Guid>(), ItSystem = new ItSystem() { Name = A<string>() } },
                Description = A<string>()
            };
        }

        private SystemRelation CreateIncomingSystemRelation(ItSystemUsage toItSystemUsage, bool withContract = false, bool withFrequency = false, bool withInterface = false)
        {
            return new SystemRelation(new ItSystemUsage() { Uuid = A<Guid>(), ItSystem = new ItSystem() { Name = A<string>() } })
            {
                Uuid = A<Guid>(),
                Reference = A<string>(),
                AssociatedContract = withContract ? new ItContract() { Uuid = A<Guid>(), Name = A<string>() } : null,
                RelationInterface = withInterface ? new ItInterface { Uuid = A<Guid>(), Name = A<string>() } : null,
                UsageFrequency = withFrequency ? new RelationFrequencyType() { Uuid = A<Guid>(), Name = A<string>() } : null,
                ToSystemUsage = toItSystemUsage,
                Description = A<string>()
            };
        }

        private void AssignExternalReferences(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.ExternalReferences = Many<string>().Select(CreateExternalReference).ToList();
            itSystemUsage.Reference = itSystemUsage.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        private ExternalReference CreateExternalReference(string title, int id)
        {
            return new ExternalReference
            {

                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = id
            };
        }

        private void AssignOrganizationalUsage(ItSystemUsage itSystemUsage)
        {
            var responsibleUsage = CreateOrganizationUnit();
            itSystemUsage.ResponsibleUsage = new ItSystemUsageOrgUnitUsage { OrganizationUnit = responsibleUsage };
            itSystemUsage.UsedBy = new[] { CreateOrganizationUnit(), CreateOrganizationUnit(), responsibleUsage }
                .Select(unit => new ItSystemUsageOrgUnitUsage { OrganizationUnit = unit }).ToList();
        }

        private OrganizationUnit CreateOrganizationUnit()
        {
            return new OrganizationUnit { Name = A<string>(), Uuid = A<Guid>() };
        }

        private void AssignKle(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.TaskRefs = Many<Guid>().Select(x => new TaskRef() { TaskKey = A<string>(), Uuid = A<Guid>() }).ToList();
            itSystemUsage.TaskRefsOptOut = Many<Guid>().Select(x => new TaskRef() { TaskKey = A<string>(), Uuid = A<Guid>() }).ToList();
        }

        private void AssignRoles(ItSystemUsage itSystemUsage)
        {
            var rights = Many<Guid>().Select(id => new ItSystemRight()
            {
                User = CreateUser(),
                Role = new ItSystemRole() { Name = A<string>(), Uuid = id }
            }).ToList();
            itSystemUsage.Rights = rights;
        }
        private static void AssertUserCount(ItSystemUsage itSystemUsage, ExpectedUsersIntervalDTO generalNumberOfExpectedUsers)
        {
            if (itSystemUsage.UserCount is UserCount.UNDECIDED or null)
            {
                Assert.Null(generalNumberOfExpectedUsers);
            }
            else
            {
                (int from, int? to) expected = itSystemUsage.UserCount switch
                {
                    UserCount.BELOWTEN => (0, 9),
                    UserCount.TENTOFIFTY => (10, 50),
                    UserCount.FIFTYTOHUNDRED => (50, 100),
                    UserCount.HUNDREDPLUS => (100, null),
                    _ => throw new ArgumentOutOfRangeException()
                };
                Assert.Equal(expected, (generalNumberOfExpectedUsers.LowerBound, generalNumberOfExpectedUsers.UpperBound));
            }
        }

        private static void AssertYesNoExtended(YesNoDontKnowChoice? actual, DataOptions? expectedFromSource)
        {
            YesNoDontKnowChoice? expected = expectedFromSource switch
            {
                DataOptions.NO => YesNoDontKnowChoice.No,
                DataOptions.YES => YesNoDontKnowChoice.Yes,
                DataOptions.DONTKNOW => YesNoDontKnowChoice.DontKnow,
                DataOptions.UNDECIDED => YesNoDontKnowChoice.Undecided,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(expectedFromSource), expectedFromSource, null)
            };
            Assert.Equal(expected, actual);
        }

        private void AssignGeneralPropertiesSection(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.LocalSystemId = A<string>();
            itSystemUsage.LocalCallName = A<string>();
            itSystemUsage.Note = A<string>();
            itSystemUsage.UserCount = A<UserCount>();
            itSystemUsage.Version = A<string>();
            itSystemUsage.ItSystemCategories = new ItSystemCategories { Name = A<string>(), Uuid = A<Guid>() };
            itSystemUsage.MainContract = new ItContractItSystemUsage { ItContract = new ItContract() { Name = A<string>(), Uuid = A<Guid>() } };
            itSystemUsage.LifeCycleStatus = A<LifeCycleStatusType>();
            itSystemUsage.Concluded = A<DateTime>();
            itSystemUsage.ExpirationDate = A<DateTime>();
            itSystemUsage.WebAccessibilityCompliance = A<YesNoPartiallyOption>();
            itSystemUsage.LastWebAccessibilityCheck = A<DateTime>();
            itSystemUsage.WebAccessibilityNotes = A<string>();

        }

        private void AssignBasicProperties(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.Id = A<int>();
            itSystemUsage.LastChanged = A<DateTime>();
            itSystemUsage.Uuid = A<Guid>();
            itSystemUsage.ObjectOwner = CreateUser();
            itSystemUsage.LastChangedByUser = CreateUser();
            itSystemUsage.ItSystem = CreateSystem();
            itSystemUsage.Organization = CreateOrganization();
        }

        private static void AssertOrganization(Organization organization, ShallowOrganizationResponseDTO dtoOrganizationContext)
        {
            AssertIdentity(organization, dtoOrganizationContext);
            Assert.Equal(organization.Cvr, dtoOrganizationContext.Cvr);
        }

        private static void AssertIdentity(ItSystemUsage sourceIdentity, IdentityNamePairResponseDTO dto)
        {
            Assert.Equal(sourceIdentity.ItSystem.Name, dto.Name);
            Assert.Equal(sourceIdentity.Uuid, dto.Uuid);
        }

        private static void AssertIdentity<T>(T sourceIdentity, IdentityNamePairResponseDTO dto) where T : IHasUuid, IHasName
        {
            Assert.Equal(sourceIdentity.Name, dto.Name);
            Assert.Equal(sourceIdentity.Uuid, dto.Uuid);
        }

        private Organization CreateOrganization()
        {
            return new Organization { Name = A<string>(), Cvr = A<string>(), Uuid = A<Guid>() };
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem
            {
                Uuid = A<Guid>(),
                Name = A<string>()
            };
        }

        private static void AssertUser(User user, IdentityNamePairResponseDTO dtoValue)
        {
            Assert.Equal((user.GetFullName(), user.Uuid), (dtoValue.Name, dtoValue.Uuid));
        }

        private User CreateUser()
        {
            return new User
            {
                Name = A<string>(),
                LastName = A<string>(),
                Uuid = A<Guid>()
            };
        }

        private static void AssertArchivePeriod(ArchivePeriod expected, JournalPeriodResponseDTO actual)
        {
            Assert.Equal(expected.Approved, actual.Approved);
            Assert.Equal(expected.StartDate, actual.StartDate);
            Assert.Equal(expected.EndDate, actual.EndDate);
            Assert.Equal(expected.UniqueArchiveId, actual.ArchiveId);
            Assert.Equal(expected.Uuid, actual.Uuid);
        }
    }
}
