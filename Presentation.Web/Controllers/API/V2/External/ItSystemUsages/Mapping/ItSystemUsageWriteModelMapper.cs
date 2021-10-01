using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
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
                GeneralProperties = request.General.FromNullable().Select(general => MapGeneralData(general, true)),
                OrganizationalUsage = request.OrganizationUsage.FromNullable().Select(orgUsage => MapOrganizationalUsage(orgUsage, true)),
                KLE = request.LocalKleDeviations.FromNullable().Select(kle => MapKle(kle, true)),
                ExternalReferences = request.ExternalReferences.FromNullable().Select(MapReferences),
                Roles = request.Roles.FromNullable().Select(MapRoles),
                GDPR = request.GDPR.FromNullable().Select(gdpr => MapGDPR(gdpr, true)),
                Archiving = request.Archiving.FromNullable().Select(archiving => MapArchiving(archiving, true))
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
                GeneralProperties = generalDataInput.FromNullable().Select(general => MapGeneralDataUpdate(general, enforceFallbackOnUndefinedProperties)),
                OrganizationalUsage = orgUsageInput.FromNullable().Select(orgUsage => MapOrganizationalUsage(orgUsage, enforceFallbackOnUndefinedProperties)),
                KLE = kleInput.FromNullable().Select(kle => MapKle(kle, enforceFallbackOnUndefinedProperties)),
                ExternalReferences = externalReferenceDataDtos.FromNullable().Select(MapReferences),
                Roles = roles.FromNullable().Select(MapRoles),
                GDPR = gdpr.FromNullable().Select(gdpr => MapGDPR(gdpr, enforceFallbackOnUndefinedProperties)),
                Archiving = archiving.FromNullable().Select(archiving => MapArchiving(archiving, enforceFallbackOnUndefinedProperties))
            };
        }

        public SystemUsageUpdateParameters FromPATCH(UpdateItSystemUsageRequestDTO request)
        {
            return MapUpdate(request, false);
        }

        private UpdatedSystemUsageGDPRProperties MapGDPR(GDPRWriteRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange<TProperty>(Expression<Func<UpdateItSystemUsageRequestDTO, TProperty>> pickProperty) => enforceFallbackIfNotProvided || ClientRequestsChangeTo(pickProperty);

            return new UpdatedSystemUsageGDPRProperties
            {
                Purpose = ShouldChange(x => x.GDPR.Purpose) 
                    ? source.Purpose.AsChangedValue() 
                    : OptionalValueChange<string>.None,
                BusinessCritical = ShouldChange(x => x.GDPR.BusinessCritical) 
                    ? MapYesNoDontKnow(source.BusinessCritical) 
                    : OptionalValueChange<DataOptions?>.None,
                HostedAt = ShouldChange(x => x.GDPR.HostedAt) 
                    ? MapEnumChoice(source.HostedAt, HostedAtMappingExtensions.ToHostedAt) 
                    : OptionalValueChange<HostedAt?>.None,
                DirectoryDocumentation = ShouldChange(x => x.GDPR.DirectoryDocumentation) 
                    ? MapLink(source.DirectoryDocumentation) 
                    : OptionalValueChange<Maybe<NamedLink>>.None,
                DataSensitivityLevels = ShouldChange(x => x.GDPR.DataSensitivityLevels) 
                    ? MapEnumList(source.DataSensitivityLevels, SensitiveDataLevelMappingExtensions.ToSensitiveDataLevel) 
                    : OptionalValueChange<Maybe<IEnumerable<SensitiveDataLevel>>>.None,
                SensitivePersonDataUuids = ShouldChange(x => x.GDPR.SensitivePersonDataUuids) 
                    ? MapCrossReferences(source.SensitivePersonDataUuids) 
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,
                RegisteredDataCategoryUuids = ShouldChange(x => x.GDPR.RegisteredDataCategoryUuids) 
                    ? MapCrossReferences(source.RegisteredDataCategoryUuids) 
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,
                TechnicalPrecautionsInPlace = ShouldChange(x => x.GDPR.TechnicalPrecautionsInPlace) 
                    ? MapYesNoDontKnow(source.TechnicalPrecautionsInPlace) 
                    : OptionalValueChange<DataOptions?>.None,
                TechnicalPrecautionsApplied = ShouldChange(x => x.GDPR.TechnicalPrecautionsApplied) 
                    ? MapEnumList(source.TechnicalPrecautionsApplied, TechnicalPrecautionMappingExtensions.ToTechnicalPrecaution) 
                    : OptionalValueChange<Maybe<IEnumerable<TechnicalPrecaution>>>.None,
                TechnicalPrecautionsDocumentation = ShouldChange(x => x.GDPR.TechnicalPrecautionsDocumentation)
                    ? MapLink(source.TechnicalPrecautionsDocumentation)
                    : OptionalValueChange<Maybe<NamedLink>>.None,
                UserSupervision = ShouldChange(x => x.GDPR.UserSupervision) 
                    ? MapYesNoDontKnow(source.UserSupervision)
                    : OptionalValueChange<DataOptions?>.None,
                UserSupervisionDate = ShouldChange(x => x.GDPR.UserSupervisionDate) 
                    ? source.UserSupervisionDate.AsChangedValue() 
                    : OptionalValueChange<DateTime?>.None,
                UserSupervisionDocumentation = ShouldChange(x => x.GDPR.UserSupervisionDocumentation) 
                    ? MapLink(source.UserSupervisionDocumentation) 
                    : OptionalValueChange<Maybe<NamedLink>>.None,
                RiskAssessmentConducted = ShouldChange(x => x.GDPR.RiskAssessmentConducted) 
                    ? MapYesNoDontKnow(source.RiskAssessmentConducted) 
                    : OptionalValueChange<DataOptions?>.None,
                RiskAssessmentConductedDate = ShouldChange(x => x.GDPR.RiskAssessmentConductedDate) 
                    ? source.RiskAssessmentConductedDate.AsChangedValue() 
                    : OptionalValueChange<DateTime?>.None,
                RiskAssessmentResult = ShouldChange(x => x.GDPR.RiskAssessmentResult) 
                    ? MapEnumChoice(source.RiskAssessmentResult, RiskLevelMappingExtensions.ToRiskLevel) 
                    : OptionalValueChange<RiskLevel?>.None,
                RiskAssessmentDocumentation = ShouldChange(x => x.GDPR.RiskAssessmentDocumentation)
                    ? MapLink(source.RiskAssessmentDocumentation) 
                    : OptionalValueChange<Maybe<NamedLink>>.None,
                RiskAssessmentNotes = ShouldChange(x => x.GDPR.RiskAssessmentNotes) 
                    ? source.RiskAssessmentNotes.AsChangedValue() 
                    : OptionalValueChange<string>.None,
                DPIAConducted = ShouldChange(x => x.GDPR.DPIAConducted) 
                    ? MapYesNoDontKnow(source.DPIAConducted) 
                    : OptionalValueChange<DataOptions?>.None,
                DPIADate = ShouldChange(x => x.GDPR.DPIADate) 
                    ? source.DPIADate.AsChangedValue() 
                    : OptionalValueChange<DateTime?>.None,
                DPIADocumentation = ShouldChange(x => x.GDPR.DPIADocumentation) 
                    ? MapLink(source.DPIADocumentation) 
                    : OptionalValueChange<Maybe<NamedLink>>.None,
                RetentionPeriodDefined = ShouldChange(x => x.GDPR.RetentionPeriodDefined) 
                    ? MapYesNoDontKnow(source.RetentionPeriodDefined) 
                    : OptionalValueChange<DataOptions?>.None,
                NextDataRetentionEvaluationDate = ShouldChange(x => x.GDPR.NextDataRetentionEvaluationDate) 
                    ? source.NextDataRetentionEvaluationDate.AsChangedValue() 
                    : OptionalValueChange<DateTime?>.None,
                DataRetentionEvaluationFrequencyInMonths = ShouldChange(x => x.GDPR.DataRetentionEvaluationFrequencyInMonths) 
                    ? source.DataRetentionEvaluationFrequencyInMonths.AsChangedValue() 
                    : OptionalValueChange<int?>.None
            };
        }

        private UpdatedSystemUsageArchivingParameters MapArchiving(ArchivingWriteRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange<TProperty>(Expression<Func<UpdateItSystemUsageRequestDTO, TProperty>> pickProperty) => enforceFallbackIfNotProvided || ClientRequestsChangeTo(pickProperty);

            return new UpdatedSystemUsageArchivingParameters()
            {
                ArchiveDuty = ShouldChange(x => x.Archiving.ArchiveDuty) 
                    ? MapEnumChoice(source.ArchiveDuty, ArchiveDutyMappingExtensions.ToArchiveDutyTypes) 
                    : OptionalValueChange<ArchiveDutyTypes?>.None,
                ArchiveTypeUuid = ShouldChange(x => x.Archiving.TypeUuid) 
                    ? (source.TypeUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue() 
                    : OptionalValueChange<Maybe<Guid>>.None,
                ArchiveLocationUuid = ShouldChange(x => x.Archiving.LocationUuid) 
                    ? (source.LocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue() 
                    : OptionalValueChange<Maybe<Guid>>.None,
                ArchiveTestLocationUuid = ShouldChange(x => x.Archiving.TestLocationUuid) 
                    ? (source.TestLocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue() 
                    : OptionalValueChange<Maybe<Guid>>.None,
                ArchiveSupplierOrganizationUuid = ShouldChange(x => x.Archiving.SupplierOrganizationUuid) 
                    ? (source.SupplierOrganizationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue() 
                    : OptionalValueChange<Maybe<Guid>>.None,
                ArchiveActive = ShouldChange(x => x.Archiving.Active) 
                    ? source.Active.AsChangedValue() 
                    : OptionalValueChange<bool?>.None,
                ArchiveNotes = ShouldChange(x => x.Archiving.Notes) 
                    ? source.Notes.AsChangedValue() 
                    : OptionalValueChange<string>.None,
                ArchiveFrequencyInMonths = ShouldChange(x => x.Archiving.FrequencyInMonths) 
                    ? source.FrequencyInMonths.AsChangedValue()
                    : OptionalValueChange<int?>.None,
                ArchiveDocumentBearing = ShouldChange(x => x.Archiving.DocumentBearing) 
                    ? source.DocumentBearing.AsChangedValue() 
                    : OptionalValueChange<bool?>.None,
                ArchiveJournalPeriods = ShouldChange(x => x.Archiving.JournalPeriods) 
                    ? source.JournalPeriods.FromNullable().Select(periods => periods.Select(MapJournalPeriod)).AsChangedValue() 
                    : OptionalValueChange<Maybe<IEnumerable<SystemUsageJournalPeriod>>>.None
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

        private UpdatedSystemUsageKLEDeviationParameters MapKle(LocalKLEDeviationsRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange<TProperty>(Expression<Func<UpdateItSystemUsageRequestDTO, TProperty>> pickProperty) => enforceFallbackIfNotProvided || ClientRequestsChangeTo(pickProperty);
            
            return new UpdatedSystemUsageKLEDeviationParameters
            {
                AddedKLEUuids = ShouldChange(x => x.LocalKleDeviations.AddedKLEUuids) 
                    ? source.AddedKLEUuids.FromNullable().AsChangedValue() 
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,

                RemovedKLEUuids = ShouldChange(x => x.LocalKleDeviations.RemovedKLEUuids)
                    ? source.RemovedKLEUuids.FromNullable().AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None
            };
        }

        /// <summary>
        /// Maps a complete update of the general data update request. Nulled sections are interpreted as intentionally reset
        /// </summary>
        /// <param name="generalData"></param>
        /// <returns></returns>
        private UpdatedSystemUsageGeneralProperties MapGeneralDataUpdate(GeneralDataUpdateRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange<TProperty>(Expression<Func<UpdateItSystemUsageRequestDTO, TProperty>> pickProperty) => enforceFallbackIfNotProvided || ClientRequestsChangeTo(pickProperty);

            var generalProperties = MapGeneralData(source, enforceFallbackIfNotProvided);
            generalProperties.MainContractUuid = ShouldChange(x => x.General.MainContractUuid) 
                ? (source.MainContractUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue() 
                : OptionalValueChange<Maybe<Guid>>.None;
            return generalProperties;
        }

        private UpdatedSystemUsageOrganizationalUseParameters MapOrganizationalUsage(OrganizationUsageWriteRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange<TProperty>(Expression<Func<UpdateItSystemUsageRequestDTO, TProperty>> pickProperty) => enforceFallbackIfNotProvided || ClientRequestsChangeTo(pickProperty);

            return new UpdatedSystemUsageOrganizationalUseParameters
            {
                ResponsibleOrganizationUnitUuid = ShouldChange(x => x.OrganizationUsage.ResponsibleOrganizationUnitUuid)
                    ? (source.ResponsibleOrganizationUnitUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,
                UsingOrganizationUnitUuids = ShouldChange(x => x.OrganizationUsage.UsingOrganizationUnitUuids)
                    ? (source.UsingOrganizationUnitUuids?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None
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

        private UpdatedSystemUsageGeneralProperties MapGeneralData(GeneralDataWriteRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            bool ShouldChange<TProperty>(Expression<Func<UpdateItSystemUsageRequestDTO, TProperty>> pickProperty) => enforceFallbackIfNotProvided || ClientRequestsChangeTo(pickProperty);

            return new UpdatedSystemUsageGeneralProperties
            {
                LocalCallName = ShouldChange(x => x.General.LocalCallName)
                    ? source.LocalCallName.AsChangedValue()
                    : OptionalValueChange<string>.None,
                LocalSystemId = ShouldChange(x => x.General.LocalSystemId)
                    ? source.LocalSystemId.AsChangedValue()
                    : OptionalValueChange<string>.None,
                Notes = ShouldChange(x => x.General.Notes) ? source.Notes.AsChangedValue() : OptionalValueChange<string>.None,
                SystemVersion = ShouldChange(x => x.General.SystemVersion)
                    ? source.SystemVersion.AsChangedValue()
                    : OptionalValueChange<string>.None,
                DataClassificationUuid = ShouldChange(x => x.General.DataClassificationUuid)
                    ? (source.DataClassificationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,
                NumberOfExpectedUsersInterval = ShouldChange(x => x.General.NumberOfExpectedUsers)
                    ? source.NumberOfExpectedUsers?.FromNullable().Select(interval =>
                          (interval.LowerBound.GetValueOrDefault(0), interval.UpperBound)) ??
                      Maybe<(int, int?)>.None.AsChangedValue()
                    : OptionalValueChange<Maybe<(int, int?)>>.None,
                EnforceActive = ShouldChange(x => x.General.Validity.EnforcedValid)
                    ? (source.Validity?.EnforcedValid.FromNullable() ?? Maybe<bool>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<bool>>.None,
                ValidFrom = ShouldChange(x => x.General.Validity.ValidFrom)
                    ? (source.Validity?.ValidFrom?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,
                ValidTo = ShouldChange(x => x.General.Validity.ValidTo)
                    ? (source.Validity?.ValidTo?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,
                AssociatedProjectUuids = ShouldChange(x => x.General.AssociatedProjectUuids)
                    ? (source.AssociatedProjectUuids?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None
            };
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