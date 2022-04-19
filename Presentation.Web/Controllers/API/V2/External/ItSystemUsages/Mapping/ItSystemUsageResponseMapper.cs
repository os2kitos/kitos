﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainServices;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Organization;

using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Generic.Validity;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public class ItSystemUsageResponseMapper : IItSystemUsageResponseMapper
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IItSystemUsageAttachedOptionRepository _itSystemUsageAttachedOptionRepository;
        private readonly ISensitivePersonalDataTypeRepository _sensitivePersonalDataTypeRepository;
        private readonly IGenericRepository<RegisterType> _registerTypesRepository;

        public ItSystemUsageResponseMapper(
            IOrganizationRepository organizationRepository,
            IItSystemUsageAttachedOptionRepository itSystemUsageAttachedOptionRepository,
            ISensitivePersonalDataTypeRepository sensitivePersonalDataTypeRepository,
            IGenericRepository<RegisterType> registerTypesRepository)
        {
            _organizationRepository = organizationRepository;
            _itSystemUsageAttachedOptionRepository = itSystemUsageAttachedOptionRepository;
            _sensitivePersonalDataTypeRepository = sensitivePersonalDataTypeRepository;
            _registerTypesRepository = registerTypesRepository;
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
                ExternalReferences = MapExternalReferences(systemUsage),
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
                //TODO: Simplify mapping once https://os2web.atlassian.net/browse/KITOSUDV-2118 is resolved
                Supplier = systemUsage
                    .SupplierId?
                    .Transform(id => _organizationRepository.GetById(id).Select(org => org.MapShallowOrganizationResponseDTO()))?
                    .GetValueOrDefault(),
                JournalPeriods = systemUsage.ArchivePeriods.Select(period => new JournalPeriodDTO
                {
                    Approved = period.Approved,
                    ArchiveId = period.UniqueArchiveId,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate
                }).ToList()
            };
        }

        private IEnumerable<SystemRelationResponseDTO> MapOutgoingSystemRelations(ItSystemUsage systemUsage)
        {
            return systemUsage.UsageRelations.Select(MapSystemRelationDTO).ToList();
        }

        private static IEnumerable<ExternalReferenceDataDTO> MapExternalReferences(ItSystemUsage systemUsage)
        {
            return systemUsage.ExternalReferences.Select(reference => MapExternalReferenceDTO(systemUsage, reference)).ToList();
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
                AssociatedProjects = systemUsage.ItProjects.Select(project => project.MapIdentityNamePairDTO()).ToList(),
                NumberOfExpectedUsers = MapExpectedUsers(systemUsage),
                SystemVersion = systemUsage.Version,
                Validity = new ValidityResponseDTO
                {
                    EnforcedValid = systemUsage.Active,
                    Valid = systemUsage.IsActive,
                    ValidFrom = systemUsage.Concluded,
                    ValidTo = systemUsage.ExpirationDate
                }
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

        public SystemRelationResponseDTO MapSystemRelationDTO(SystemRelation systemRelation)
        {
            return new()
            {
                Uuid = systemRelation.Uuid,
                Description = systemRelation.Description,
                UrlReference = systemRelation.Reference,
                AssociatedContract = systemRelation.AssociatedContract?.MapIdentityNamePairDTO(),
                RelationFrequency = systemRelation.UsageFrequency?.MapIdentityNamePairDTO(),
                RelationInterface = systemRelation.RelationInterface?.MapIdentityNamePairDTO(),
                ToSystemUsage = systemRelation.ToSystemUsage?.MapIdentityNamePairDTO()
            };
        }

        private static ExternalReferenceDataDTO MapExternalReferenceDTO(ItSystemUsage systemUsage, ExternalReference reference)
        {
            return new()
            {
                DocumentId = reference.ExternalReferenceId,
                Title = reference.Title,
                Url = reference.URL,
                MasterReference = systemUsage.Reference?.Id.Equals(reference.Id) == true
            };
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
                UserCount.BELOWTEN => new ExpectedUsersIntervalDTO { LowerBound = 0, UpperBound = 9 },
                UserCount.TENTOFIFTY => new ExpectedUsersIntervalDTO { LowerBound = 10, UpperBound = 50 },
                UserCount.FIFTYTOHUNDRED => new ExpectedUsersIntervalDTO { LowerBound = 50, UpperBound = 100 },
                UserCount.HUNDREDPLUS => new ExpectedUsersIntervalDTO { LowerBound = 100 },
                _ => throw new ArgumentOutOfRangeException(nameof(systemUsage.UserCount))
            };
        }
    }
}