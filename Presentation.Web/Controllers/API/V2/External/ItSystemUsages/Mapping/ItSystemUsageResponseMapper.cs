using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainServices;
using Core.DomainServices.Repositories.GDPR;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Presentation.Web.Controllers.API.V2.External.Generic;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public class ItSystemUsageResponseMapper : IItSystemUsageResponseMapper
    {
        private readonly IItSystemUsageAttachedOptionRepository _itSystemUsageAttachedOptionRepository;
        private readonly ISensitivePersonalDataTypeRepository _sensitivePersonalDataTypeRepository;
        private readonly IGenericRepository<RegisterType> _registerTypesRepository;
        private readonly IExternalReferenceResponseMapper _externalReferenceResponseMapper;

        public ItSystemUsageResponseMapper(
            IItSystemUsageAttachedOptionRepository itSystemUsageAttachedOptionRepository,
            ISensitivePersonalDataTypeRepository sensitivePersonalDataTypeRepository,
            IGenericRepository<RegisterType> registerTypesRepository,
            IExternalReferenceResponseMapper externalReferenceResponseMapper)
        {
            _itSystemUsageAttachedOptionRepository = itSystemUsageAttachedOptionRepository;
            _sensitivePersonalDataTypeRepository = sensitivePersonalDataTypeRepository;
            _registerTypesRepository = registerTypesRepository;
            _externalReferenceResponseMapper = externalReferenceResponseMapper;
        }

        public ItSystemUsageResponseDTO MapSystemUsageDTO(ItSystemUsage systemUsage)
        {
            return new ItSystemUsageResponseDTO
            {
                Uuid = systemUsage.Uuid,
                SystemContext = systemUsage.ItSystem.MapIdentityNamePairDTO(),
                OrganizationContext = systemUsage.Organization.MapShallowOrganizationResponseDTO(),
                CreatedBy = systemUsage.ObjectOwner?.MapIdentityNamePairDTO(),
                LastModified = systemUsage.LastChanged,
                LastModifiedBy = systemUsage.LastChangedByUser?.MapIdentityNamePairDTO(),
                General = MapGeneral(systemUsage),
                Roles = MapRoles(systemUsage),
                LocalKLEDeviations = MapKle(systemUsage),
                OrganizationUsage = MapOrganizationUsage(systemUsage),
                ExternalReferences = _externalReferenceResponseMapper.MapExternalReferences(systemUsage.ExternalReferences),
                OutgoingSystemRelations = MapOutgoingSystemRelations(systemUsage),
                Archiving = MapArchiving(systemUsage),
                GDPR = MapGDPR(systemUsage)
            };
        }

        private GDPRRegistrationsResponseDTO MapGDPR(ItSystemUsage systemUsage)
        {
            var personDataTypesMap = new Lazy<IDictionary<int, SensitivePersonalDataType>>(() => _sensitivePersonalDataTypeRepository.GetSensitivePersonalDataTypes().ToDictionary(type => type.Id));
            var registerTypesMap = new Lazy<IDictionary<int, RegisterType>>(() => _registerTypesRepository.Get().ToDictionary(type => type.Id));
            var attachedOptions = _itSystemUsageAttachedOptionRepository.GetBySystemUsageId(systemUsage.Id).ToList();

            return new GDPRRegistrationsResponseDTO
            {
                Purpose = systemUsage.GeneralPurpose,
                BusinessCritical = MapYesNoExtended(systemUsage.isBusinessCritical),
                DPIAConducted = MapYesNoExtended(systemUsage.DPIA),
                DPIADate = systemUsage.DPIADateFor,
                DPIADocumentation = MapSimpleLink(systemUsage.DPIASupervisionDocumentationUrlName, systemUsage.DPIASupervisionDocumentationUrl),
                HostedAt = MapHosting(systemUsage),
                DirectoryDocumentation = MapSimpleLink(systemUsage.LinkToDirectoryUrlName, systemUsage.LinkToDirectoryUrl),
                DataSensitivityLevels = systemUsage
                    .SensitiveDataLevels
                    .Select(MapDataSensitivity)
                    .Where(level => level.HasValue)
                    .Select(level => level.Value)
                    .ToList(),
                SpecificPersonalData = systemUsage.PersonalDataOptions.Select(x => x.PersonalData.ToGDPRPersonalDataChoice()).ToList(),
                SensitivePersonData = attachedOptions
                    .Where(option => option.OptionType == OptionType.SENSITIVEPERSONALDATA)
                    .Where(option => personDataTypesMap.Value.ContainsKey(option.OptionId))
                    .Select(option => personDataTypesMap.Value[option.OptionId].MapIdentityNamePairDTO())
                    .ToList(),
                RegisteredDataCategories = attachedOptions
                    .Where(option => option.OptionType == OptionType.REGISTERTYPEDATA)
                    .Where(option => registerTypesMap.Value.ContainsKey(option.OptionId))
                    .Select(option => registerTypesMap.Value[option.OptionId].MapIdentityNamePairDTO())
                    .ToList(),
                TechnicalPrecautionsInPlace = MapYesNoExtended(systemUsage.precautions),
                TechnicalPrecautionsApplied = MapPrecautions(systemUsage).ToList(),
                TechnicalPrecautionsDocumentation = MapSimpleLink(systemUsage.TechnicalSupervisionDocumentationUrlName, systemUsage.TechnicalSupervisionDocumentationUrl),
                RetentionPeriodDefined = MapYesNoExtended(systemUsage.answeringDataDPIA),
                DataRetentionEvaluationFrequencyInMonths = systemUsage.numberDPIA,
                NextDataRetentionEvaluationDate = systemUsage.DPIAdeleteDate,
                RiskAssessmentConducted = MapYesNoExtended(systemUsage.riskAssessment),
                RiskAssessmentConductedDate = systemUsage.riskAssesmentDate,
                RiskAssessmentDocumentation = MapSimpleLink(systemUsage.RiskSupervisionDocumentationUrlName, systemUsage.RiskSupervisionDocumentationUrl),
                RiskAssessmentNotes = systemUsage.noteRisks,
                RiskAssessmentResult = MapRiskAssessment(systemUsage),
                PlannedRiskAssessmentDate = systemUsage.PlannedRiskAssessmentDate,
                UserSupervision = MapYesNoExtended(systemUsage.UserSupervision),
                UserSupervisionDate = systemUsage.UserSupervisionDate,
                UserSupervisionDocumentation = MapSimpleLink(systemUsage.UserSupervisionDocumentationUrlName, systemUsage.UserSupervisionDocumentationUrl)
            };
        }

        private ArchivingRegistrationsResponseDTO MapArchiving(ItSystemUsage systemUsage)
        {
            return new ArchivingRegistrationsResponseDTO
            {
                Active = systemUsage.ArchiveFromSystem,
                Notes = systemUsage.ArchiveNotes,
                ArchiveDuty = MapArchiveDuty(systemUsage),
                DocumentBearing = systemUsage.Registertype,
                FrequencyInMonths = systemUsage.ArchiveFreq,
                Location = systemUsage.ArchiveLocation?.MapIdentityNamePairDTO(),
                TestLocation = systemUsage.ArchiveTestLocation?.MapIdentityNamePairDTO(),
                Type = systemUsage.ArchiveType?.MapIdentityNamePairDTO(),
                Supplier = systemUsage.ArchiveSupplier?.MapShallowOrganizationResponseDTO(),
                JournalPeriods = systemUsage.ArchivePeriods.Select(period => MapJournalPeriodResponseDto(period)).ToList()
            };
        }

        public JournalPeriodResponseDTO MapJournalPeriodResponseDto(ArchivePeriod period)
        {
            return new JournalPeriodResponseDTO
            {
                Uuid = period.Uuid,
                Approved = period.Approved,
                ArchiveId = period.UniqueArchiveId,
                StartDate = period.StartDate,
                EndDate = period.EndDate
            };
        }

        private IEnumerable<OutgoingSystemRelationResponseDTO> MapOutgoingSystemRelations(ItSystemUsage systemUsage)
        {
            return systemUsage.UsageRelations.Select(MapOutgoingSystemRelationDTO).ToList();
        }

        private static OrganizationUsageResponseDTO MapOrganizationUsage(ItSystemUsage systemUsage)
        {
            return new OrganizationUsageResponseDTO
            {
                ResponsibleOrganizationUnit = systemUsage.ResponsibleUsage?.OrganizationUnit?.MapIdentityNamePairDTO(),
                UsingOrganizationUnits = systemUsage.UsedBy.Select(x => x.OrganizationUnit.MapIdentityNamePairDTO()).ToList()
            };
        }

        private static LocalKLEDeviationsResponseDTO MapKle(ItSystemUsage systemUsage)
        {
            return new LocalKLEDeviationsResponseDTO
            {
                AddedKLE = systemUsage.TaskRefs.Select(taskRef => taskRef.MapIdentityNamePairDTO()).ToList(),
                RemovedKLE = systemUsage.TaskRefsOptOut.Select(taskRef => taskRef.MapIdentityNamePairDTO()).ToList()
            };
        }

        private static List<RoleAssignmentResponseDTO> MapRoles(ItSystemUsage systemUsage)
        {
            return systemUsage.Rights.Select(ToRoleResponseDTO).ToList();
        }

        private static GeneralDataResponseDTO MapGeneral(ItSystemUsage systemUsage)
        {
            return new GeneralDataResponseDTO
            {
                LocalCallName = systemUsage.LocalCallName,
                LocalSystemId = systemUsage.LocalSystemId,
                Notes = systemUsage.Note,
                MainContract = systemUsage.MainContract?.ItContract?.MapIdentityNamePairDTO(),
                DataClassification = systemUsage.ItSystemCategories?.MapIdentityNamePairDTO(),
                NumberOfExpectedUsers = MapExpectedUsers(systemUsage),
                SystemVersion = systemUsage.Version,
                Validity = new ItSystemUsageValidityResponseDTO
                {
                    Valid = systemUsage.CheckSystemValidity().Result,
                    ValidAccordingToValidityPeriod = systemUsage.IsActiveAccordingToDateFields,
                    ValidAccordingToLifeCycle = systemUsage.IsActiveAccordingToLifeCycle,
                    ValidAccordingToMainContract = systemUsage.IsActiveAccordingToMainContract,
                    LifeCycleStatus = MapLifeCycleStatus(systemUsage),
                    ValidFrom = systemUsage.Concluded,
                    ValidTo = systemUsage.ExpirationDate
                },
                ContainsAITechnology = systemUsage.ContainsAITechnology?.ToYesNoUndecidedChoice(),
                WebAccessibilityCompliance = systemUsage.WebAccessibilityCompliance?.ToYesNoPartiallyChoice(),
                LastWebAccessibilityCheck = systemUsage.LastWebAccessibilityCheck,
                WebAccessibilityNotes = systemUsage.WebAccessibilityNotes
            };
        }

        private static RiskLevelChoice? MapRiskAssessment(ItSystemUsage systemUsage)
        {
            return systemUsage.preriskAssessment?.ToRiskLevelChoice();
        }

        private static IEnumerable<TechnicalPrecautionChoice> MapPrecautions(ItSystemUsage systemUsage)
        {
            return systemUsage.GetTechnicalPrecautions().Select(precaution => precaution.ToTechnicalPrecautionChoice());
        }

        private static DataSensitivityLevelChoice? MapDataSensitivity(ItSystemUsageSensitiveDataLevel dataLevel)
        {
            return dataLevel?.SensitivityDataLevel.ToDataSensitivityLevelChoice();
        }

        private static HostingChoice? MapHosting(ItSystemUsage systemUsage)
        {
            return systemUsage.HostedAt?.ToHostingChoice();
        }

        private static SimpleLinkDTO MapSimpleLink(string name, string url)
        {
            return new()
            {
                Name = name,
                Url = url
            };
        }

        private static YesNoDontKnowChoice? MapYesNoExtended(DataOptions? input)
        {
            return input?.ToYesNoDontKnowChoice();
        }

        private static ArchiveDutyChoice? MapArchiveDuty(ItSystemUsage systemUsage)
        {
            return systemUsage.ArchiveDuty?.ToArchiveDutyChoice();
        }

        private static LifeCycleStatusChoice? MapLifeCycleStatus(ItSystemUsage systemUsage)
        {
            return systemUsage.LifeCycleStatus?.ToLifeCycleStatusChoice();
        }

        public GeneralSystemRelationResponseDTO MapGeneralSystemRelationDTO(SystemRelation systemRelation)
        {
            var dto = new GeneralSystemRelationResponseDTO
            {
                ToSystemUsage = systemRelation.ToSystemUsage?.MapIdentityNamePairDTO(),
                FromSystemUsage = systemRelation.FromSystemUsage?.MapIdentityNamePairDTO()
            };
            return MapSharedRelationProperties(systemRelation, dto);
        }

        public OutgoingSystemRelationResponseDTO MapOutgoingSystemRelationDTO(SystemRelation systemRelation)
        {
            var dto = new OutgoingSystemRelationResponseDTO
            {
                ToSystemUsage = systemRelation.ToSystemUsage?.MapIdentityNamePairDTO()
            };
            return MapSharedRelationProperties(systemRelation, dto);
        }

        public IncomingSystemRelationResponseDTO MapIncomingSystemRelationDTO(SystemRelation systemRelation)
        {
            var dto = new IncomingSystemRelationResponseDTO
            {
                FromSystemUsage = systemRelation.FromSystemUsage?.MapIdentityNamePairDTO()
            };
            return MapSharedRelationProperties(systemRelation, dto);
        }

        private static T MapSharedRelationProperties<T>(SystemRelation systemRelation, T dto) where T : BaseSystemRelationResponseDTO
        {
            dto.Uuid = systemRelation.Uuid;
            dto.Description = systemRelation.Description;
            dto.UrlReference = systemRelation.Reference;
            dto.AssociatedContract = systemRelation.AssociatedContract?.MapIdentityNamePairDTO();
            dto.RelationFrequency = systemRelation.UsageFrequency?.MapIdentityNamePairDTO();
            dto.RelationInterface = systemRelation.RelationInterface?.MapIdentityNamePairDTO();
            return dto;
        }

        private static RoleAssignmentResponseDTO ToRoleResponseDTO(ItSystemRight right)
        {
            return new()
            {
                Role = right.Role.MapIdentityNamePairDTO(),
                User = right.User.MapIdentityNamePairDTO()
            };
        }

        private static ExpectedUsersIntervalDTO MapExpectedUsers(ItSystemUsage systemUsage)
        {
            return systemUsage.UserCount switch
            {
                null or UserCount.UNDECIDED => null,
                UserCount.BELOWTEN => new ExpectedUsersIntervalDTO { LowerBound = 0, UpperBound = 9 },
                UserCount.TENTOFIFTY => new ExpectedUsersIntervalDTO { LowerBound = 10, UpperBound = 50 },
                UserCount.FIFTYTOHUNDRED => new ExpectedUsersIntervalDTO { LowerBound = 50, UpperBound = 100 },
                UserCount.HUNDREDPLUS => new ExpectedUsersIntervalDTO { LowerBound = 100 },
                _ => throw new ArgumentOutOfRangeException(nameof(systemUsage.UserCount))
            };
        }
    }
}