using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public class DataProcessingRegistrationWriteModelMapper : WriteModelMapperBase, IDataProcessingRegistrationWriteModelMapper
    {
        public DataProcessingRegistrationWriteModelMapper(ICurrentHttpRequest currentHttpRequest)
            : base(currentHttpRequest)
        {
        }

        public DataProcessingRegistrationModificationParameters FromPOST(CreateDataProcessingRegistrationRequestDTO dto)
        {
            return MapCreate(dto, false);
        }

        public DataProcessingRegistrationModificationParameters FromPUT(UpdateDataProcessingRegistrationRequestDTO dto)
        {
            return MapUpdate(dto, true);
        }

        public DataProcessingRegistrationModificationParameters FromPATCH(UpdateDataProcessingRegistrationRequestDTO dto)
        {
            return MapUpdate(dto, false);
        }

        private DataProcessingRegistrationModificationParameters MapCreate(
            CreateDataProcessingRegistrationRequestDTO dto, bool enforceFallbackIfNotProvided)
        {
            var parameters = Map<CreateDataProcessingRegistrationRequestDTO, ExternalReferenceDataWriteRequestDTO>(dto, enforceFallbackIfNotProvided);
            parameters.ExternalReferences = MapCreateReferences(dto);

            return parameters;
        }

        private DataProcessingRegistrationModificationParameters MapUpdate(
            UpdateDataProcessingRegistrationRequestDTO dto, bool enforceFallbackIfNotProvided)
        {
            var parameters = Map<UpdateDataProcessingRegistrationRequestDTO, UpdateExternalReferenceDataWriteRequestDTO>(dto, enforceFallbackIfNotProvided);
            parameters.ExternalReferences = MapUpdateReferences(dto);

            return parameters;
        }

        private DataProcessingRegistrationModificationParameters Map<TDto, TExternalReferenceDto>(TDto dto, bool enforceFallbackIfNotProvided)
            where TDto : DataProcessingRegistrationWriteRequestDTO, IHasNameExternal, IHasExternalReference<TExternalReferenceDto>
            where TExternalReferenceDto : ExternalReferenceDataWriteRequestDTO
        {
            TSection WithResetDataIfSectionIsNotDefined<TSection>(TSection deserializedValue, Expression<Func<TDto, TSection>> propertySelection) where TSection : new() => WithResetDataIfPropertyIsDefined(deserializedValue, propertySelection, enforceFallbackIfNotProvided);
            TSection WithResetDataIfSectionIsNotDefinedWithFallback<TSection>(TSection deserializedValue, Expression<Func<TDto, TSection>> propertySelection, Func<TSection> fallbackFactory) => WithResetDataIfPropertyIsDefined(deserializedValue, propertySelection, fallbackFactory, enforceFallbackIfNotProvided);

            dto.General = WithResetDataIfSectionIsNotDefined(dto.General, x => x.General);
            dto.SystemUsageUuids = WithResetDataIfSectionIsNotDefinedWithFallback(dto.SystemUsageUuids, x => x.SystemUsageUuids, () => new List<Guid>());
            dto.Oversight = WithResetDataIfSectionIsNotDefined(dto.Oversight, x => x.Oversight);
            dto.Roles = WithResetDataIfSectionIsNotDefinedWithFallback(dto.Roles, x => x.Roles, Array.Empty<RoleAssignmentRequestDTO>);
            dto.ExternalReferences = WithResetDataIfSectionIsNotDefinedWithFallback(dto.ExternalReferences, x => x.ExternalReferences, Array.Empty<TExternalReferenceDto>);

            return new DataProcessingRegistrationModificationParameters
            {
                Name = (ClientRequestsChangeTo<IHasNameExternal>(x => x.Name) || enforceFallbackIfNotProvided) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                General = dto.General.FromNullable().Select(generalData => MapGeneral(generalData, enforceFallbackIfNotProvided)),
                SystemUsageUuids = dto.SystemUsageUuids.FromNullable(),
                Oversight = dto.Oversight.FromNullable().Select(oversight => MapOversight(oversight, enforceFallbackIfNotProvided)),
                Roles = dto.Roles.FromNullable().Select(MapRoles)
            };
        }

        private Maybe<IEnumerable<UpdatedExternalReferenceProperties>> MapCreateReferences(CreateDataProcessingRegistrationRequestDTO dto)
        {
            return dto.ExternalReferences.FromNullable().Select(BaseMapCreateReferences);
        }

        private Maybe<IEnumerable<UpdatedExternalReferenceProperties>> MapUpdateReferences(UpdateDataProcessingRegistrationRequestDTO dto)
        {
            return dto.ExternalReferences.FromNullable().Select(BaseMapUpdateReferences);
        }

        private UpdatedDataProcessingRegistrationGeneralDataParameters MapGeneral(DataProcessingRegistrationGeneralDataWriteRequestDTO dto, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<DataProcessingRegistrationWriteRequestDTO>(enforceFallbackIfNotProvided);
            return new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = rule.MustUpdate(x => x.General.DataResponsibleUuid)
                    ? dto.DataResponsibleUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                DataResponsibleRemark = rule.MustUpdate(x => x.General.DataResponsibleRemark)
                    ? dto.DataResponsibleRemark.AsChangedValue()
                    : OptionalValueChange<string>.None,

                IsAgreementConcluded = rule.MustUpdate(x => x.General.IsAgreementConcluded)
                    ? (dto.IsAgreementConcluded?.ToYesNoIrrelevantOption()).AsChangedValue()
                    : OptionalValueChange<YesNoIrrelevantOption?>.None,

                IsAgreementConcludedRemark = rule.MustUpdate(x => x.General.IsAgreementConcludedRemark)
                    ? dto.IsAgreementConcludedRemark.AsChangedValue()
                    : OptionalValueChange<string>.None,

                AgreementConcludedAt = rule.MustUpdate(x => x.General.AgreementConcludedAt)
                    ? dto.AgreementConcludedAt.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                BasisForTransferUuid = rule.MustUpdate(x => x.General.BasisForTransferUuid)
                    ? dto.BasisForTransferUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                TransferToInsecureThirdCountries = rule.MustUpdate(x => x.General.TransferToInsecureThirdCountries)
                    ? (dto.TransferToInsecureThirdCountries?.ToYesNoUndecidedOption()).AsChangedValue()
                    : OptionalValueChange<YesNoUndecidedOption?>.None,

                InsecureCountriesSubjectToDataTransferUuids =
                    rule.MustUpdate(x => x.General.InsecureCountriesSubjectToDataTransferUuids)
                        ? dto.InsecureCountriesSubjectToDataTransferUuids.FromNullable().AsChangedValue()
                        : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,

                DataProcessorUuids = rule.MustUpdate(x => x.General.DataProcessorUuids)
                    ? dto.DataProcessorUuids.FromNullable().AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,

                HasSubDataProcessors = rule.MustUpdate(x => x.General.HasSubDataProcessors)
                    ? (dto.HasSubDataProcessors?.ToYesNoUndecidedOption()).AsChangedValue()
                    : OptionalValueChange<YesNoUndecidedOption?>.None,

                SubDataProcessors = rule.MustUpdate(x => x.General.SubDataProcessors)
                    ? dto.SubDataProcessors.FromNullable().Select<IEnumerable<SubDataProcessorParameter>>(sdps => sdps.Select(ToSubDataProcessorParameter).ToList()).AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<SubDataProcessorParameter>>>.None,

                MainContractUuid = rule.MustUpdate(x => x.General.MainContractUuid)
                    ? dto.MainContractUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,
                ResponsibleUnitUuid = rule.MustUpdate(x => x.General.ResponsibleOrganizationUnitUuid) ? dto.ResponsibleOrganizationUnitUuid.AsChangedValue() : OptionalValueChange<Guid?>.None
            };
        }

        private static SubDataProcessorParameter ToSubDataProcessorParameter(DataProcessorRegistrationSubDataProcessorWriteRequestDTO sdp)
        {
            return new SubDataProcessorParameter(
                sdp.DataProcessorOrganizationUuid,
                sdp.BasisForTransferUuid,
                sdp.TransferToInsecureThirdCountry?.ToYesNoUndecidedOption(),
                sdp.InsecureThirdCountrySubjectToDataProcessingUuid);
        }

        private UpdatedDataProcessingRegistrationOversightDataParameters MapOversight(DataProcessingRegistrationOversightWriteRequestDTO dto, bool enforceFallbackIfNotProvided)
        {
            var rule = CreateChangeRule<DataProcessingRegistrationWriteRequestDTO>(enforceFallbackIfNotProvided);

            return new UpdatedDataProcessingRegistrationOversightDataParameters
            {
                OversightOptionUuids = rule.MustUpdate(x => x.Oversight.OversightOptionUuids)
                    ? dto.OversightOptionUuids.FromNullable().AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<Guid>>>.None,

                OversightOptionsRemark = rule.MustUpdate(x => x.Oversight.OversightOptionsRemark)
                    ? dto.OversightOptionsRemark.AsChangedValue()
                    : OptionalValueChange<string>.None,

                OversightInterval = rule.MustUpdate(x => x.Oversight.OversightInterval)
                    ? (dto.OversightInterval?.ToIntervalOption()).AsChangedValue()
                    : OptionalValueChange<YearMonthIntervalOption?>.None,

                OversightIntervalRemark = rule.MustUpdate(x => x.Oversight.OversightIntervalRemark)
                    ? dto.OversightIntervalRemark.AsChangedValue()
                    : OptionalValueChange<string>.None,

                IsOversightCompleted = rule.MustUpdate(x => x.Oversight.IsOversightCompleted)
                    ? (dto.IsOversightCompleted?.ToYesNoUndecidedOption()).AsChangedValue()
                    : OptionalValueChange<YesNoUndecidedOption?>.None,

                OversightCompletedRemark = rule.MustUpdate(x => x.Oversight.OversightCompletedRemark)
                    ? dto.OversightCompletedRemark.AsChangedValue()
                    : OptionalValueChange<string>.None,

                OversightScheduledInspectionDate = rule.MustUpdate(x => x.Oversight.OversightScheduledInspectionDate)
                    ? dto.OversightScheduledInspectionDate.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                OversightDates = rule.MustUpdate(x => x.Oversight.OversightDates)
                    ? dto.OversightDates
                        .FromNullable()
                        .Select(x => x
                            .Select(y => new UpdatedDataProcessingRegistrationOversightDate()
                            {
                                CompletedAt = y.CompletedAt,
                                Remark = y.Remark
                            })).AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>>.None
            };
        }

        private static UpdatedDataProcessingRegistrationRoles MapRoles(IEnumerable<RoleAssignmentRequestDTO> roles)
        {
            var roleAssignmentResponseDtos = roles.ToList();

            return new UpdatedDataProcessingRegistrationRoles
            {
                UserRolePairs = BaseMapRoleAssignments(roleAssignmentResponseDtos)
            };
        }
    }
}