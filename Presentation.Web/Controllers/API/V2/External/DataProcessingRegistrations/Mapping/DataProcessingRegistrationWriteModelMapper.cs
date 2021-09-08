using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared.Write;

using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public class DataProcessingRegistrationWriteModelMapper : IDataProcessingRegistrationWriteModelMapper
    {
        public DataProcessingRegistrationModificationParameters FromPOST(DataProcessingRegistrationWriteRequestDTO dto)
        {
            return Map(dto);
        }

        public DataProcessingRegistrationModificationParameters FromPUT(DataProcessingRegistrationWriteRequestDTO dto)
        {
            dto.General ??= new DataProcessingRegistrationGeneralDataWriteRequestDTO();
            dto.SystemUsageUuids ??= Array.Empty<Guid>();
            dto.Oversight ??= new DataProcessingRegistrationOversightWriteRequestDTO();
            dto.Roles ??= Array.Empty<RoleAssignmentRequestDTO>();
            dto.ExternalReferences ??= Array.Empty<ExternalReferenceDataDTO>();
            return Map(dto);
        }
        private DataProcessingRegistrationModificationParameters Map(DataProcessingRegistrationWriteRequestDTO dto)
        {
            return new DataProcessingRegistrationModificationParameters
            {
                Name = dto.Name.AsChangedValue(),
                General = dto.General.FromNullable().Select(MapGeneral),
                SystemUsageUuids = dto.SystemUsageUuids.FromNullable(),
                Oversight = dto.Oversight.FromNullable().Select(MapOversight),
                Roles = dto.Roles.FromNullable().Select(MapRoles),
                ExternalReferences = dto.ExternalReferences.FromNullable().Select(MapReferences)
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
            }).ToList();
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
                UserRolePairs = roleAssignmentResponseDtos.Any() ?
                    roleAssignmentResponseDtos.Select(x => new UserRolePair
                    {
                        RoleUuid = x.RoleUuid,
                        UserUuid = x.UserUuid
                    }).FromNullable().AsChangedValue() :
                    Maybe<IEnumerable<UserRolePair>>.None
            };
        }
    }
}