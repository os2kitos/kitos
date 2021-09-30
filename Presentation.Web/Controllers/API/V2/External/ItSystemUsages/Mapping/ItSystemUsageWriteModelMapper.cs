using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.ItSystem.DataTypes;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public class ItSystemUsageWriteModelMapper : WriteModelMapperBase, IItSystemUsageWriteModelMapper
    {
        public ItSystemUsageWriteModelMapper(ICurrentHttpRequest currentHttpRequest)
            : base(currentHttpRequest)
        {
        }

        public SystemUsageUpdateParameters FromPOST(CreateItSystemUsageRequestDTO request)
        {
            var parameters = new SystemUsageUpdateParameters
            {
                GeneralProperties = request.General.FromNullable().Select(general => MapGeneralData(general, new UpdatedSystemUsageGeneralProperties(), true)),
                OrganizationalUsage = request.OrganizationUsage.FromNullable().Select(MapOrganizationalUsage),
                KLE = request.LocalKleDeviations.FromNullable().Select(MapKle),
                ExternalReferences = request.ExternalReferences.FromNullable().Select(MapReferences),
                Roles = request.Roles.FromNullable().Select(MapRoles),
                GDPR = request.GDPR.FromNullable().Select(MapGDPR),
                Archiving = request.Archiving.FromNullable().Select(MapArchiving)
            };

            return parameters;
        }

        public SystemUsageUpdateParameters FromPUT(UpdateItSystemUsageRequestDTO request)
        {
            return MapUpdate(request, true);
        }

        private SystemUsageUpdateParameters MapUpdate(UpdateItSystemUsageRequestDTO request, bool enforceFallbackOnUndefinedProperties)
        {
            var generalDataInput = WithResetDataIfPropertyIsDefined(request.General, nameof(UpdateItSystemUsageRequestDTO.General), enforceFallbackOnUndefinedProperties);
            var orgUsageInput = WithResetDataIfPropertyIsDefined(request.OrganizationUsage, nameof(UpdateItSystemUsageRequestDTO.OrganizationUsage), enforceFallbackOnUndefinedProperties);
            var kleInput = WithResetDataIfPropertyIsDefined(request.LocalKleDeviations, nameof(UpdateItSystemUsageRequestDTO.LocalKleDeviations), enforceFallbackOnUndefinedProperties);
            var roles = WithResetDataIfPropertyIsDefined(request.Roles, nameof(UpdateItSystemUsageRequestDTO.Roles), () => new List<RoleAssignmentRequestDTO>(), enforceFallbackOnUndefinedProperties);
            var externalReferenceDataDtos = WithResetDataIfPropertyIsDefined(request.ExternalReferences, nameof(UpdateItSystemUsageRequestDTO.ExternalReferences), () => new List<ExternalReferenceDataDTO>(), enforceFallbackOnUndefinedProperties);
            var gdpr = WithResetDataIfPropertyIsDefined(request.GDPR, nameof(UpdateItSystemUsageRequestDTO.GDPR), enforceFallbackOnUndefinedProperties);
            var archiving = WithResetDataIfPropertyIsDefined(request.Archiving, nameof(UpdateItSystemUsageRequestDTO.Archiving), enforceFallbackOnUndefinedProperties);

            return new SystemUsageUpdateParameters
            {
                GeneralProperties = generalDataInput.FromNullable().Select(general => MapGeneralDataUpdate(general, new UpdatedSystemUsageGeneralProperties(), enforceFallbackOnUndefinedProperties)),
                OrganizationalUsage = orgUsageInput.FromNullable().Select(MapOrganizationalUsage),
                KLE = kleInput.FromNullable().Select(MapKle),
                ExternalReferences = externalReferenceDataDtos.FromNullable().Select(MapReferences),
                Roles = roles.FromNullable().Select(MapRoles),
                GDPR = gdpr.FromNullable().Select(MapGDPR),
                Archiving = archiving.FromNullable().Select(MapArchiving)
            };
        }

        public SystemUsageUpdateParameters FromPATCH(UpdateItSystemUsageRequestDTO request)
        {
            return MapUpdate(request, false);
        }

        private static UpdatedSystemUsageGDPRProperties MapGDPR(GDPRWriteRequestDTO request)
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

        private static UpdatedSystemUsageArchivingParameters MapArchiving(ArchivingWriteRequestDTO archiving)
        {
            return new UpdatedSystemUsageArchivingParameters()
            {
                ArchiveDuty = MapEnumChoice(archiving.ArchiveDuty, ArchiveDutyMappingExtensions.ToArchiveDutyTypes),
                ArchiveTypeUuid = (archiving.TypeUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveLocationUuid = (archiving.LocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveTestLocationUuid = (archiving.TestLocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveSupplierOrganizationUuid = (archiving.SupplierOrganizationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                ArchiveActive = archiving.Active.AsChangedValue(),
                ArchiveNotes = archiving.Notes.AsChangedValue(),
                ArchiveFrequencyInMonths = archiving.FrequencyInMonths.AsChangedValue(),
                ArchiveDocumentBearing = archiving.DocumentBearing.AsChangedValue(),
                ArchiveJournalPeriods = archiving.JournalPeriods.FromNullable().Select(periods => periods.Select(MapJournalPeriod)).AsChangedValue()
            };
        }
        private static SystemUsageJournalPeriod MapJournalPeriod(JournalPeriodDTO journalPeriod)
        {
            return new SystemUsageJournalPeriod
            {
                Approved = journalPeriod.Approved,
                ArchiveId = journalPeriod.ArchiveId,
                EndDate = journalPeriod.EndDate,
                StartDate = journalPeriod.StartDate
            };
        }

        private IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> references)
        {
            return BaseMapReferences(references);
        }

        private static UpdatedSystemUsageKLEDeviationParameters MapKle(LocalKLEDeviationsRequestDTO kle)
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
        private UpdatedSystemUsageGeneralProperties MapGeneralDataUpdate(GeneralDataUpdateRequestDTO source, UpdatedSystemUsageGeneralProperties destination, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange(string propertyName) => ClientRequestsChangeTo(propertyName) || enforceFallbackIfNotProvided;

            var generalProperties = MapGeneralData(source, new UpdatedSystemUsageGeneralProperties(), enforceFallbackIfNotProvided);
            destination.MainContractUuid = ShouldChange(nameof(GeneralDataUpdateRequestDTO.MainContractUuid)) ? source.MainContractUuid?.FromNullable().AsChangedValue() : OptionalValueChange<Maybe<Guid>>.None;

            return generalProperties;
        }

        private static UpdatedSystemUsageOrganizationalUseParameters MapOrganizationalUsage(OrganizationUsageWriteRequestDTO input)
        {
            return new UpdatedSystemUsageOrganizationalUseParameters
            {
                ResponsibleOrganizationUnitUuid = (input.ResponsibleOrganizationUnitUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                UsingOrganizationUnitUuids = (input.UsingOrganizationUnitUuids?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue()
            };
        }

        private static UpdatedSystemUsageRoles MapRoles(IEnumerable<RoleAssignmentRequestDTO> roles)
        {
            var roleAssignmentResponseDtos = roles.ToList();

            return new UpdatedSystemUsageRoles
            {
                UserRolePairs = BaseMapRoleAssignments(roleAssignmentResponseDtos)
            };
        }

        private UpdatedSystemUsageGeneralProperties MapGeneralData(GeneralDataWriteRequestDTO source, UpdatedSystemUsageGeneralProperties destination, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange(string propertyName) => ClientRequestsChangeTo(propertyName) || enforceFallbackIfNotProvided;

            destination.LocalCallName = ShouldChange(nameof(GeneralDataWriteRequestDTO.LocalCallName)) ? source.LocalCallName.AsChangedValue() : OptionalValueChange<string>.None;
            destination.LocalSystemId = ShouldChange(nameof(GeneralDataWriteRequestDTO.LocalSystemId)) ? source.LocalSystemId.AsChangedValue() : OptionalValueChange<string>.None;
            destination.Notes = ShouldChange(nameof(GeneralDataWriteRequestDTO.Notes)) ? source.Notes.AsChangedValue() : OptionalValueChange<string>.None;
            destination.SystemVersion = ShouldChange(nameof(GeneralDataWriteRequestDTO.SystemVersion)) ? source.SystemVersion.AsChangedValue() : OptionalValueChange<string>.None;
            destination.DataClassificationUuid = ShouldChange(nameof(GeneralDataWriteRequestDTO.DataClassificationUuid)) ? source.DataClassificationUuid?.FromNullable().AsChangedValue() : OptionalValueChange<Maybe<Guid>>.None;
            destination.NumberOfExpectedUsersInterval = ShouldChange(nameof(GeneralDataWriteRequestDTO.NumberOfExpectedUsers)) ? source.NumberOfExpectedUsers?.FromNullable().Select(interval => (interval.LowerBound.GetValueOrDefault(0), interval.UpperBound)) ?? Maybe<(int, int?)>.None.AsChangedValue() : OptionalValueChange<Maybe<(int, int?)>>.None;
            destination.EnforceActive = ShouldChange(nameof(GeneralDataWriteRequestDTO.Validity.EnforcedValid)) ? source.Validity?.EnforcedValid.FromNullable().AsChangedValue() : OptionalValueChange<Maybe<bool>>.None;
            destination.ValidFrom = ShouldChange(nameof(GeneralDataWriteRequestDTO.Validity.ValidFrom)) ? source.Validity?.ValidFrom?.FromNullable().AsChangedValue() : OptionalValueChange<Maybe<DateTime>>.None;
            destination.ValidTo = ShouldChange(nameof(GeneralDataWriteRequestDTO.Validity.ValidTo)) ? source.Validity?.ValidTo?.FromNullable().AsChangedValue() : OptionalValueChange<Maybe<DateTime>>.None;
            destination.AssociatedProjectUuids = ShouldChange(nameof(GeneralDataWriteRequestDTO.AssociatedProjectUuids)) ? source.AssociatedProjectUuids?.FromNullable().AsChangedValue() : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;

            return destination;
        }

        public SystemRelationParameters MapRelation(SystemRelationWriteRequestDTO relationData)
        {
            if (relationData == null)
                throw new ArgumentNullException(nameof(relationData));

            return new SystemRelationParameters
            (
                relationData.ToSystemUsageUuid,
                relationData.RelationInterfaceUuid,
                relationData.AssociatedContractUuid,
                relationData.RelationFrequencyUuid,
                relationData.Description,
                relationData.UrlReference
                );
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