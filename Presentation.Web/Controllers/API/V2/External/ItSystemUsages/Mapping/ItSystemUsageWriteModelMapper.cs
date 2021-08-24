using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.ItSystem.DataTypes;
using Infrastructure.Services.Types;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public class ItSystemUsageWriteModelMapper : IItSystemUsageWriteModelMapper
    {
        /// <summary>
        /// Creates a update parameters for POST operation. Undefined sections will be ignored
        /// </summary>
        /// <param name="request"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public SystemUsageUpdateParameters FromPOST(CreateItSystemUsageRequestDTO request)
        {
            return new SystemUsageUpdateParameters
            {
                GeneralProperties = request.General.FromNullable().Select(MapGeneralData),
                OrganizationalUsage = request.OrganizationUsage.FromNullable().Select(MapOrganizationalUsage),
                KLE = request.LocalKleDeviations.FromNullable().Select(MapKle),
                ExternalReferences = request.ExternalReferences.FromNullable().Select(MapReferences),
                Roles = request.Roles.FromNullable().Select(MapRoles),
                GDPR = request.GDPR.FromNullable().Select(MapGDPR),
                Archiving = request.Archiving.FromNullable().Select(MapArchiving)
            };
        }

        /// <summary>
        /// Creates a complete update object where all values are defined and fallbacks to null are used for sections which are missing
        /// </summary>
        /// <param name="request"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public SystemUsageUpdateParameters FromPUT(UpdateItSystemUsageRequestDTO request)
        {
            var generalDataInput = request.General ?? new GeneralDataUpdateRequestDTO();
            var orgUsageInput = request.OrganizationUsage ?? new OrganizationUsageWriteRequestDTO();
            var kleInput = request.LocalKleDeviations ?? new LocalKLEDeviationsRequestDTO();
            var roles = request.Roles ?? new List<RoleAssignmentRequestDTO>();
            var externalReferenceDataDtos = request.ExternalReferences ?? new List<ExternalReferenceDataDTO>();
            var gdpr = request.GDPR ?? new GDPRWriteRequestDTO();
            var archiving = request.Archiving ?? new ArchivingWriteRequestDTO();
            return new SystemUsageUpdateParameters
            {
                GeneralProperties = generalDataInput.FromNullable().Select(MapGeneralDataUpdate),
                OrganizationalUsage = orgUsageInput.FromNullable().Select(MapOrganizationalUsage),
                KLE = kleInput.FromNullable().Select(MapKle),
                ExternalReferences = externalReferenceDataDtos.FromNullable().Select(MapReferences),
                Roles = roles.FromNullable().Select(MapRoles),
                GDPR = gdpr.FromNullable().Select(MapGDPR),
                Archiving = archiving.FromNullable().Select(MapArchiving)
            };
        }

        public UpdatedSystemUsageGDPRProperties MapGDPR(GDPRWriteRequestDTO request)
        {
            return new UpdatedSystemUsageGDPRProperties
            {
                Purpose = request.Purpose.AsChangedValue(),
                BusinessCritical = MapYesNoDontKnow(request.BusinessCritical),
                HostedAt = MapEnumChoice(request.HostedAt, HostedAtMappingExtensions.ToHostedAt),
                DirectoryDocumentation = MapLink(request.DirectoryDocumentation),
                DataSensitivityLevels = MapEnumList(request.DataSensitivityLevels, SensitiveDataLevelMappingExtensions.ToSensitiveDataLevel),
                SensitivePersonDataUuids = MapCrossReferences(request.SensitivePersonDataUuids),
                RegisteredDataCategoryUuids = MapCrossReferences(request.RegisteredDataCategoryUuids),
                TechnicalPrecautionsInPlace = MapYesNoDontKnow(request.TechnicalPrecautionsInPlace),
                TechnicalPrecautionsApplied = MapEnumList(request.TechnicalPrecautionsApplied, TechnicalPrecautionMappingExtensions.ToTechnicalPrecaution),
                TechnicalPrecautionsDocumentation = MapLink(request.TechnicalPrecautionsDocumentation),
                UserSupervision = MapYesNoDontKnow(request.UserSupervision),
                UserSupervisionDate = request.UserSupervisionDate.AsChangedValue(),
                UserSupervisionDocumentation = MapLink(request.UserSupervisionDocumentation),
                RiskAssessmentConducted = MapYesNoDontKnow(request.RiskAssessmentConducted),
                RiskAssessmentConductedDate = request.RiskAssessmentConductedDate.AsChangedValue(),
                RiskAssessmentResult = MapEnumChoice(request.RiskAssessmentResult, RiskLevelMappingExtensions.ToRiskLevel),
                RiskAssessmentDocumentation = MapLink(request.RiskAssessmentDocumentation),
                RiskAssessmentNotes = request.RiskAssessmentNotes.AsChangedValue(),
                DPIAConducted = MapYesNoDontKnow(request.DPIAConducted),
                DPIADate = request.DPIADate.AsChangedValue(),
                DPIADocumentation = MapLink(request.DPIADocumentation),
                RetentionPeriodDefined = MapYesNoDontKnow(request.RetentionPeriodDefined),
                NextDataRetentionEvaluationDate = request.NextDataRetentionEvaluationDate.AsChangedValue(),
                DataRetentionEvaluationFrequencyInMonths = request.DataRetentionEvaluationFrequencyInMonths.AsChangedValue()
            };
        }
        public UpdatedSystemUsageArchivingParameters MapArchiving(ArchivingWriteRequestDTO archiving)
        {
            return new UpdatedSystemUsageArchivingParameters()
            {
                ArchiveDuty = MapEnumChoice(archiving.ArchiveDuty, ArchiveDutyMappingExtensions.ToArchiveDutyTypes),
                ArchiveTypeUuid = (archiving.ArchiveTypeUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveLocationUuid = (archiving.ArchiveLocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveTestLocationUuid = (archiving.ArchiveTestLocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveSupplierOrganizationUuid = (archiving.ArchiveSupplierOrganizationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveActive = archiving.ArchiveActive.AsChangedValue(),
                ArchiveNotes = archiving.ArchiveNotes.AsChangedValue(),
                ArchiveFrequencyInMonths = archiving.ArchiveFrequencyInMonths.AsChangedValue(),
                ArchiveDocumentBearing = archiving.ArchiveDocumentBearing.AsChangedValue(),
                ArchiveJournalPeriods = archiving.ArchiveJournalPeriods.FromNullable().Select(periods => periods.Select(MapJournalPeriod)).AsChangedValue()
            };
        }
        private static SystemUsageJournalPeriod MapJournalPeriod(JournalPeriodDTO journalPeriod)
        {
            return new SystemUsageJournalPeriod()
            {
                Approved = journalPeriod.Approved,
                ArchiveId = journalPeriod.ArchiveId,
                EndDate = journalPeriod.EndDate,
                StartDate = journalPeriod.StartDate
            };
        }
        public IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> references)
        {
            return references.Select(x => new UpdatedExternalReferenceProperties
            {
                Title = x.Title,
                DocumentId = x.DocumentId,
                Url = x.Url,
                MasterReference = x.MasterReference
            });
        }
        public UpdatedSystemUsageKLEDeviationParameters MapKle(LocalKLEDeviationsRequestDTO kle)
        {
            return new UpdatedSystemUsageKLEDeviationParameters
            {
                AddedKLEUuids = kle.AddedKLEUuids.FromNullable().AsChangedValue(),
                RemovedKLEUuids = kle.RemovedKLEUuids.FromNullable().AsChangedValue()
            };
        }

        /// <summary>
        /// Maps a complete update of the general data update request. Nulled sections are interpreted as intentionally reset
        /// </summary>
        /// <param name="generalData"></param>
        /// <returns></returns>
        public UpdatedSystemUsageGeneralProperties MapGeneralDataUpdate(GeneralDataUpdateRequestDTO generalData)
        {
            var generalProperties = MapGeneralData(generalData);
            generalProperties.MainContractUuid = (generalData.MainContractUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue();
            return generalProperties;
        }

        public UpdatedSystemUsageOrganizationalUseParameters MapOrganizationalUsage(OrganizationUsageWriteRequestDTO input)
        {
            return new UpdatedSystemUsageOrganizationalUseParameters
            {
                ResponsibleOrganizationUnitUuid = (input.ResponsibleOrganizationUnitUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                UsingOrganizationUnitUuids = (input.UsingOrganizationUnitUuids?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue()
            };
        }

        public UpdatedSystemUsageRoles MapRoles(IEnumerable<RoleAssignmentRequestDTO> roles)
        {
            var roleAssignmentResponseDtos = roles.ToList();

            return new UpdatedSystemUsageRoles
            {
                UserRolePairs = (roleAssignmentResponseDtos.Any() ?
                    Maybe<IEnumerable<UserRolePair>>.Some(roleAssignmentResponseDtos.Select(x => new UserRolePair
                    {
                        RoleUuid = x.RoleUuid,
                        UserUuid = x.UserUuid
                    })) :
                    Maybe<IEnumerable<UserRolePair>>.None).AsChangedValue()
            };
        }

        public UpdatedSystemUsageGeneralProperties MapGeneralData(GeneralDataWriteRequestDTO generalData)
        {
            return new UpdatedSystemUsageGeneralProperties
            {
                LocalCallName = generalData.LocalCallName.AsChangedValue(),
                LocalSystemId = generalData.LocalSystemId.AsChangedValue(),
                Notes = generalData.Notes.AsChangedValue(),
                SystemVersion = generalData.SystemVersion.AsChangedValue(),
                DataClassificationUuid = (generalData.DataClassificationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                NumberOfExpectedUsersInterval = (generalData
                    .NumberOfExpectedUsers?
                    .FromNullable()
                    .Select(interval => (interval.LowerBound.GetValueOrDefault(0), interval.UpperBound)) ?? Maybe<(int, int?)>.None)
                    .AsChangedValue(),
                EnforceActive = ((generalData.Validity?.EnforcedValid)?.FromNullable() ?? Maybe<bool>.None).AsChangedValue(),
                ValidFrom = (generalData.Validity?.ValidFrom?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue(),
                ValidTo = (generalData.Validity?.ValidTo?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue(),
                AssociatedProjectUuids = (generalData.AssociatedProjectUuids?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue()
            };
        }

        private static ChangedValue<Maybe<IEnumerable<TOut>>> MapEnumList<TIn, TOut>(IEnumerable<TIn> list, Func<TIn, TOut> mapWith)
        {
            return (list?.FromNullable().Select(dataSensitivityLevelChoices => dataSensitivityLevelChoices.Select(mapWith)) ?? Maybe<IEnumerable<TOut>>.None).AsChangedValue();
        }

        private static ChangedValue<TOut?> MapEnumChoice<TIn, TOut>(TIn? choice, Func<TIn, TOut> mapWith) where TIn : struct where TOut : struct
        {
            return choice.HasValue ? mapWith(choice.Value) : new ChangedValue<TOut?>(null);
        }

        private static ChangedValue<Maybe<IEnumerable<Guid>>> MapCrossReferences(IEnumerable<Guid> crossReferences)
        {
            return (crossReferences?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue();
        }

        private static ChangedValue<DataOptions?> MapYesNoDontKnow(YesNoDontKnowChoice? choice)
        {
            return MapEnumChoice(choice, DataOptionsMappingExtensions.ToDataOptions);
        }

        private static ChangedValue<Maybe<NamedLink>> MapLink(SimpleLinkDTO simpleLinkDto)
        {
            return (simpleLinkDto?.FromNullable().Select(linkDto => new NamedLink(linkDto.Name, linkDto.Url)) ?? Maybe<NamedLink>.None).AsChangedValue();
        }
    }
}