using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Shared;

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
            return Map(dto, false);
        }

        public DataProcessingRegistrationModificationParameters FromPUT(UpdateDataProcessingRegistrationRequestDTO dto)
        {
            return Map(dto, true);
        }

        public DataProcessingRegistrationModificationParameters FromPATCH(UpdateDataProcessingRegistrationRequestDTO dto)
        {
            return Map(dto, false);
        }

        private DataProcessingRegistrationModificationParameters Map<T>(T dto, bool enforceFallbackIfNotProvided) where T : DataProcessingRegistrationWriteRequestDTO, IHasNameExternal
        {
            dto.General = WithResetDataIfPropertyIsDefined(dto.General, nameof(DataProcessingRegistrationWriteRequestDTO.General), enforceFallbackIfNotProvided);
            dto.SystemUsageUuids = WithResetDataIfPropertyIsDefined(dto.SystemUsageUuids, nameof(DataProcessingRegistrationWriteRequestDTO.SystemUsageUuids), () => new List<Guid>(), enforceFallbackIfNotProvided);
            dto.Oversight = WithResetDataIfPropertyIsDefined(dto.Oversight, nameof(DataProcessingRegistrationWriteRequestDTO.Oversight), enforceFallbackIfNotProvided);
            dto.Roles = WithResetDataIfPropertyIsDefined(dto.Roles, nameof(DataProcessingRegistrationWriteRequestDTO.Roles), Array.Empty<RoleAssignmentRequestDTO>, enforceFallbackIfNotProvided);
            dto.ExternalReferences = WithResetDataIfPropertyIsDefined(dto.ExternalReferences, nameof(DataProcessingRegistrationWriteRequestDTO.ExternalReferences), Array.Empty<ExternalReferenceDataDTO>, enforceFallbackIfNotProvided);

            return new DataProcessingRegistrationModificationParameters
            {
                Name = (ClientRequestsChangeTo(nameof(IHasNameExternal.Name)) || enforceFallbackIfNotProvided) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                General = dto.General.FromNullable().Select(MapGeneral),
                SystemUsageUuids = dto.SystemUsageUuids.FromNullable(),
                Oversight = dto.Oversight.FromNullable().Select(MapOversight),
                Roles = dto.Roles.FromNullable().Select(MapRoles),
                ExternalReferences = dto.ExternalReferences.FromNullable().Select(MapReferences)
            };
        }

        public IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> references)
        {
            return BaseMapReferences(references); ;
        }

        public UpdatedDataProcessingRegistrationGeneralDataParameters MapGeneral(DataProcessingRegistrationGeneralDataWriteRequestDTO dto)
        {
            return new UpdatedDataProcessingRegistrationGeneralDataParameters
            {
                DataResponsibleUuid = dto.DataResponsibleUuid.AsChangedValue(),
                DataResponsibleRemark = dto.DataResponsibleRemark.AsChangedValue(),
                IsAgreementConcluded = (dto.IsAgreementConcluded?.ToYesNoIrrelevantOption()).AsChangedValue(),
                IsAgreementConcludedRemark = dto.IsAgreementConcludedRemark.AsChangedValue(),
                AgreementConcludedAt = dto.AgreementConcludedAt.AsChangedValue(),
                BasisForTransferUuid = dto.BasisForTransferUuid.AsChangedValue(),
                TransferToInsecureThirdCountries = (dto.TransferToInsecureThirdCountries?.ToYesNoUndecidedOption()).AsChangedValue(),
                InsecureCountriesSubjectToDataTransferUuids = dto.InsecureCountriesSubjectToDataTransferUuids.FromNullable().AsChangedValue(),
                DataProcessorUuids = dto.DataProcessorUuids.FromNullable().AsChangedValue(),
                HasSubDataProcessors = (dto.HasSubDataProcessors?.ToYesNoUndecidedOption()).AsChangedValue(),
                SubDataProcessorUuids = dto.SubDataProcessorUuids.FromNullable().AsChangedValue()
            };
        }

        public UpdatedDataProcessingRegistrationOversightDataParameters MapOversight(DataProcessingRegistrationOversightWriteRequestDTO dto)
        {
            return new UpdatedDataProcessingRegistrationOversightDataParameters
            {
                OversightOptionUuids = dto.OversightOptionUuids.FromNullable().AsChangedValue(),
                OversightOptionsRemark = dto.OversightOptionsRemark.AsChangedValue(),
                OversightInterval = (dto.OversightInterval?.ToIntervalOption()).AsChangedValue(),
                OversightIntervalRemark = dto.OversightIntervalRemark.AsChangedValue(),
                IsOversightCompleted = (dto.IsOversightCompleted?.ToYesNoUndecidedOption()).AsChangedValue(),
                OversightCompletedRemark = dto.OversightCompletedRemark.AsChangedValue(),
                OversightDates = dto.OversightDates
                    .FromNullable()
                    .Select(x => x
                        .Select(y => new UpdatedDataProcessingRegistrationOversightDate()
                        {
                            CompletedAt = y.CompletedAt,
                            Remark = y.Remark
                        })).AsChangedValue()
            };
        }

        public UpdatedDataProcessingRegistrationRoles MapRoles(IEnumerable<RoleAssignmentRequestDTO> roles)
        {
            var roleAssignmentResponseDtos = roles.ToList();

            return new UpdatedDataProcessingRegistrationRoles
            {
                UserRolePairs = BaseMapRoleAssignments(roleAssignmentResponseDtos)
            };
        }
    }
}