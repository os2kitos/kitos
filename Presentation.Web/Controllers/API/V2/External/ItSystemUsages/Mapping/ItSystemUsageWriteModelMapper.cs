using System;
using System.Collections.Generic;
using System.Linq;
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
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Presentation.Web.Controllers.API.V2.External.Generic;

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
                Archiving = request.Archiving
                    .FromNullable()
                    .Select(archiving => MapBaseArchiving(archiving, true))
                    .Select(archiving => MapJournalPeriods(archiving, request.Archiving, true))
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
            var externalReferenceDataDtos = WithResetDataIfPropertyIsDefined(request.ExternalReferences, nameof(UpdateItSystemUsageRequestDTO.ExternalReferences), () => new List<UpdateExternalReferenceDataWriteRequestDTO>(), enforceFallbackOnUndefinedProperties);
            var gdpr = WithResetDataIfPropertyIsDefined(request.GDPR, nameof(UpdateItSystemUsageRequestDTO.GDPR), enforceFallbackOnUndefinedProperties);
            var archiving = WithResetDataIfPropertyIsDefined(request.Archiving, nameof(UpdateItSystemUsageRequestDTO.Archiving), enforceFallbackOnUndefinedProperties);

            return new SystemUsageUpdateParameters
            {
                GeneralProperties = generalDataInput.FromNullable().Select(general => MapGeneralDataUpdate(general, enforceFallbackOnUndefinedProperties)),
                OrganizationalUsage = orgUsageInput.FromNullable().Select(orgUsage => MapOrganizationalUsage(orgUsage, enforceFallbackOnUndefinedProperties)),
                KLE = kleInput.FromNullable().Select(kle => MapKle(kle, enforceFallbackOnUndefinedProperties)),
                ExternalReferences = externalReferenceDataDtos.FromNullable().Select(MapUpdateReferences),
                Roles = roles.FromNullable().Select(MapRoles),
                GDPR = gdpr.FromNullable().Select(gdpr => MapGDPR(gdpr, enforceFallbackOnUndefinedProperties)),
                Archiving = archiving
                    .FromNullable()
                    .Select(a => MapBaseArchiving(a, enforceFallbackOnUndefinedProperties))
                    .Select(a => MapUpdatedJournalPeriods(a, archiving, enforceFallbackOnUndefinedProperties))
            };
        }

        public SystemUsageUpdateParameters FromPATCH(UpdateItSystemUsageRequestDTO request)
        {
            return MapUpdate(request, false);
        }

        private UpdatedSystemUsageGDPRProperties MapGDPR(GDPRWriteRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            return new UpdatedSystemUsageGDPRProperties
            {
                Purpose = rule.MustUpdate(x => x.GDPR.Purpose)
                    ? source.Purpose.AsChangedValue()
                    : OptionalValueChange<string>.None,

                BusinessCritical = rule.MustUpdate(x => x.GDPR.BusinessCritical)
                    ? MapYesNoDontKnow(source.BusinessCritical)
                    : OptionalValueChange<DataOptions?>.None,

                HostedAt = rule.MustUpdate(x => x.GDPR.HostedAt)
                    ? MapEnumChoice(source.HostedAt, HostedAtMappingExtensions.ToHostedAt)
                    : OptionalValueChange<HostedAt?>.None,

                DirectoryDocumentation = rule.MustUpdate(x => x.GDPR.DirectoryDocumentation)
                    ? MapLink(source.DirectoryDocumentation)
                    : OptionalValueChange<Maybe<NamedLink>>.None,

                DataSensitivityLevels = rule.MustUpdate(x => x.GDPR.DataSensitivityLevels)
                    ? MapEnumList(source.DataSensitivityLevels, SensitiveDataLevelMappingExtensions.ToSensitiveDataLevel)
                    : OptionalValueChange<Maybe<IEnumerable<SensitiveDataLevel>>>.None,

                SensitivePersonDataUuids = rule.MustUpdate(x => x.GDPR.SensitivePersonDataUuids)
                    ? MapCrossReferences(source.SensitivePersonDataUuids)
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,

                PersonalDataOptions = rule.MustUpdate(x => x.GDPR.SpecificPersonalData)
                    ? MapEnumList(source.SpecificPersonalData, GDPRPersonalDataMappingExtensions.ToGDPRPersonalDataOption)
                    : OptionalValueChange<Maybe<IEnumerable<GDPRPersonalDataOption>>>.None,

                RegisteredDataCategoryUuids = rule.MustUpdate(x => x.GDPR.RegisteredDataCategoryUuids)
                    ? MapCrossReferences(source.RegisteredDataCategoryUuids)
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,

                TechnicalPrecautionsInPlace = rule.MustUpdate(x => x.GDPR.TechnicalPrecautionsInPlace)
                    ? MapYesNoDontKnow(source.TechnicalPrecautionsInPlace)
                    : OptionalValueChange<DataOptions?>.None,

                TechnicalPrecautionsApplied = rule.MustUpdate(x => x.GDPR.TechnicalPrecautionsApplied)
                    ? MapEnumList(source.TechnicalPrecautionsApplied, TechnicalPrecautionMappingExtensions.ToTechnicalPrecaution)
                    : OptionalValueChange<Maybe<IEnumerable<TechnicalPrecaution>>>.None,

                TechnicalPrecautionsDocumentation = rule.MustUpdate(x => x.GDPR.TechnicalPrecautionsDocumentation)
                    ? MapLink(source.TechnicalPrecautionsDocumentation)
                    : OptionalValueChange<Maybe<NamedLink>>.None,

                UserSupervision = rule.MustUpdate(x => x.GDPR.UserSupervision)
                    ? MapYesNoDontKnow(source.UserSupervision)
                    : OptionalValueChange<DataOptions?>.None,

                UserSupervisionDate = rule.MustUpdate(x => x.GDPR.UserSupervisionDate)
                    ? source.UserSupervisionDate.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                UserSupervisionDocumentation = rule.MustUpdate(x => x.GDPR.UserSupervisionDocumentation)
                    ? MapLink(source.UserSupervisionDocumentation)
                    : OptionalValueChange<Maybe<NamedLink>>.None,

                RiskAssessmentConducted = rule.MustUpdate(x => x.GDPR.RiskAssessmentConducted)
                    ? MapYesNoDontKnow(source.RiskAssessmentConducted)
                    : OptionalValueChange<DataOptions?>.None,

                RiskAssessmentConductedDate = rule.MustUpdate(x => x.GDPR.RiskAssessmentConductedDate)
                    ? source.RiskAssessmentConductedDate.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                RiskAssessmentResult = rule.MustUpdate(x => x.GDPR.RiskAssessmentResult)
                    ? MapEnumChoice(source.RiskAssessmentResult, RiskLevelMappingExtensions.ToRiskLevel)
                    : OptionalValueChange<RiskLevel?>.None,

                RiskAssessmentDocumentation = rule.MustUpdate(x => x.GDPR.RiskAssessmentDocumentation)
                    ? MapLink(source.RiskAssessmentDocumentation)
                    : OptionalValueChange<Maybe<NamedLink>>.None,

                RiskAssessmentNotes = rule.MustUpdate(x => x.GDPR.RiskAssessmentNotes)
                    ? source.RiskAssessmentNotes.AsChangedValue()
                    : OptionalValueChange<string>.None,

                PlannedRiskAssessmentDate = rule.MustUpdate(x => x.GDPR.PlannedRiskAssessmentDate)
                    ? source.PlannedRiskAssessmentDate.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                DPIAConducted = rule.MustUpdate(x => x.GDPR.DPIAConducted)
                    ? MapYesNoDontKnow(source.DPIAConducted)
                    : OptionalValueChange<DataOptions?>.None,

                DPIADate = rule.MustUpdate(x => x.GDPR.DPIADate)
                    ? source.DPIADate.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                DPIADocumentation = rule.MustUpdate(x => x.GDPR.DPIADocumentation)
                    ? MapLink(source.DPIADocumentation)
                    : OptionalValueChange<Maybe<NamedLink>>.None,

                RetentionPeriodDefined = rule.MustUpdate(x => x.GDPR.RetentionPeriodDefined)
                    ? MapYesNoDontKnow(source.RetentionPeriodDefined)
                    : OptionalValueChange<DataOptions?>.None,

                NextDataRetentionEvaluationDate = rule.MustUpdate(x => x.GDPR.NextDataRetentionEvaluationDate)
                    ? source.NextDataRetentionEvaluationDate.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                DataRetentionEvaluationFrequencyInMonths = rule.MustUpdate(x => x.GDPR.DataRetentionEvaluationFrequencyInMonths)
                    ? source.DataRetentionEvaluationFrequencyInMonths.AsChangedValue()
                    : OptionalValueChange<int?>.None
            };
        }

        private UpdatedSystemUsageArchivingParameters MapBaseArchiving(BaseArchivingWriteRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            return new UpdatedSystemUsageArchivingParameters()
            {
                ArchiveDuty = rule.MustUpdate(x => x.Archiving.ArchiveDuty)
                    ? MapEnumChoice(source.ArchiveDuty, ArchiveDutyMappingExtensions.ToArchiveDutyTypes)
                    : OptionalValueChange<ArchiveDutyTypes?>.None,

                ArchiveTypeUuid = rule.MustUpdate(x => x.Archiving.TypeUuid)
                    ? (source.TypeUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,

                ArchiveLocationUuid = rule.MustUpdate(x => x.Archiving.LocationUuid)
                    ? (source.LocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,

                ArchiveTestLocationUuid = rule.MustUpdate(x => x.Archiving.TestLocationUuid)
                    ? (source.TestLocationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,

                ArchiveSupplierOrganizationUuid = rule.MustUpdate(x => x.Archiving.SupplierOrganizationUuid)
                    ? (source.SupplierOrganizationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,

                ArchiveActive = rule.MustUpdate(x => x.Archiving.Active)
                    ? source.Active.AsChangedValue()
                    : OptionalValueChange<bool?>.None,

                ArchiveNotes = rule.MustUpdate(x => x.Archiving.Notes)
                    ? source.Notes.AsChangedValue()
                    : OptionalValueChange<string>.None,

                ArchiveFrequencyInMonths = rule.MustUpdate(x => x.Archiving.FrequencyInMonths)
                    ? source.FrequencyInMonths.AsChangedValue()
                    : OptionalValueChange<int?>.None,

                ArchiveDocumentBearing = rule.MustUpdate(x => x.Archiving.DocumentBearing)
                    ? source.DocumentBearing.AsChangedValue()
                    : OptionalValueChange<bool?>.None,
            };
        }
        private UpdatedSystemUsageArchivingParameters MapJournalPeriods(UpdatedSystemUsageArchivingParameters parameters, ArchivingCreationRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            parameters.ArchiveJournalPeriods = rule.MustUpdate(x => x.Archiving.JournalPeriods)
                ? source.JournalPeriods.FromNullable().Select(periods => periods.Select(MapNewJournalPeriod)).AsChangedValue()
                : OptionalValueChange<Maybe<IEnumerable<SystemUsageJournalPeriodUpdate>>>.None;

            return parameters;
        }
        private UpdatedSystemUsageArchivingParameters MapUpdatedJournalPeriods(UpdatedSystemUsageArchivingParameters parameters, ArchivingUpdateRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            parameters.ArchiveJournalPeriods = rule.MustUpdate(x => x.Archiving.JournalPeriods)
                ? source.JournalPeriods.FromNullable().Select(periods => periods.Select(MapUpdatedJournalPeriod)).AsChangedValue()
                : OptionalValueChange<Maybe<IEnumerable<SystemUsageJournalPeriodUpdate>>>.None;

            return parameters;
        }
        private static SystemUsageJournalPeriodUpdate MapNewJournalPeriod(JournalPeriodDTO newJournalPeriod)
        {
            var update = new SystemUsageJournalPeriodUpdate();
            MapJournalPeriodProperties(newJournalPeriod, update);
            return update;
        }

        private static SystemUsageJournalPeriodUpdate MapUpdatedJournalPeriod(JournalPeriodUpdateRequestDTO journalPeriodUpdate)
        {
            var update = new SystemUsageJournalPeriodUpdate();
            update.Uuid = journalPeriodUpdate.Uuid;
            MapJournalPeriodProperties(journalPeriodUpdate, update);
            return update;
        }

        public SystemUsageJournalPeriodProperties MapJournalPeriodProperties(JournalPeriodDTO input)
        {
            var properties = new SystemUsageJournalPeriodProperties();
            MapJournalPeriodProperties(input, properties);
            return properties;
        }

        private static void MapJournalPeriodProperties(JournalPeriodDTO journalPeriod, SystemUsageJournalPeriodProperties update)
        {
            update.Approved = journalPeriod.Approved;
            update.ArchiveId = journalPeriod.ArchiveId;
            update.EndDate = journalPeriod.EndDate;
            update.StartDate = journalPeriod.StartDate;
        }

        private IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataWriteRequestDTO> references)
        {
            return BaseMapCreateReferences(references);
        }

        private IEnumerable<UpdatedExternalReferenceProperties> MapUpdateReferences(IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> references)
        {
            return BaseMapUpdateReferences(references);
        }

        private UpdatedSystemUsageKLEDeviationParameters MapKle(LocalKLEDeviationsRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            return new UpdatedSystemUsageKLEDeviationParameters
            {
                AddedKLEUuids = rule.MustUpdate(x => x.LocalKleDeviations.AddedKLEUuids)
                    ? source.AddedKLEUuids.FromNullable().AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,

                RemovedKLEUuids = rule.MustUpdate(x => x.LocalKleDeviations.RemovedKLEUuids)
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
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            var generalProperties = MapGeneralData(source, enforceFallbackIfNotProvided);

            generalProperties.MainContractUuid = rule.MustUpdate(x => x.General.MainContractUuid)
                ? (source.MainContractUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                : OptionalValueChange<Maybe<Guid>>.None;

            return generalProperties;
        }

        private UpdatedSystemUsageOrganizationalUseParameters MapOrganizationalUsage(OrganizationUsageWriteRequestDTO source, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            return new UpdatedSystemUsageOrganizationalUseParameters
            {
                ResponsibleOrganizationUnitUuid = rule.MustUpdate(x => x.OrganizationUsage.ResponsibleOrganizationUnitUuid)
                    ? (source.ResponsibleOrganizationUnitUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,

                UsingOrganizationUnitUuids = rule.MustUpdate(x => x.OrganizationUsage.UsingOrganizationUnitUuids)
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
            var rule = CreateChangeRule<UpdateItSystemUsageRequestDTO>(enforceFallbackIfNotProvided);

            return new UpdatedSystemUsageGeneralProperties
            {
                LocalCallName = rule.MustUpdate(x => x.General.LocalCallName)
                    ? source.LocalCallName.AsChangedValue()
                    : OptionalValueChange<string>.None,

                LocalSystemId = rule.MustUpdate(x => x.General.LocalSystemId)
                    ? source.LocalSystemId.AsChangedValue()
                    : OptionalValueChange<string>.None,

                Notes = rule.MustUpdate(x => x.General.Notes) ? source.Notes.AsChangedValue() : OptionalValueChange<string>.None,

                SystemVersion = rule.MustUpdate(x => x.General.SystemVersion)
                    ? source.SystemVersion.AsChangedValue()
                    : OptionalValueChange<string>.None,

                DataClassificationUuid = rule.MustUpdate(x => x.General.DataClassificationUuid)
                    ? (source.DataClassificationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<Guid>>.None,

                NumberOfExpectedUsersInterval = rule.MustUpdate(x => x.General.NumberOfExpectedUsers)
                    ? source.NumberOfExpectedUsers?.FromNullable().Select(interval =>
                          (interval.LowerBound, interval.UpperBound)) ??
                      Maybe<(int, int?)>.None.AsChangedValue()
                    : OptionalValueChange<Maybe<(int, int?)>>.None,

                LifeCycleStatus = rule.MustUpdate(x => x.General.Validity.LifeCycleStatus)
                    ? MapEnumChoice(source.Validity?.LifeCycleStatus, LifeCycleStatusMappingExtensions.ToLifeCycleStatusType)
                    : OptionalValueChange<LifeCycleStatusType?>.None,

                ValidFrom = rule.MustUpdate(x => x.General.Validity.ValidFrom)
                    ? (source.Validity?.ValidFrom?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,

                ValidTo = rule.MustUpdate(x => x.General.Validity.ValidTo)
                    ? (source.Validity?.ValidTo?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,

                ContainsAITechnology = rule.MustUpdate(x => x.General.ContainsAITechnology)
                    ? (source.ContainsAITechnology?.ToYesNoUndecidedOption() ?? Maybe<YesNoUndecidedOption>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<YesNoUndecidedOption>>.None
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

        public ExternalReferenceProperties MapExternalReference(ExternalReferenceDataWriteRequestDTO externalReferenceData)
        {
            return MapCommonReference(externalReferenceData);
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