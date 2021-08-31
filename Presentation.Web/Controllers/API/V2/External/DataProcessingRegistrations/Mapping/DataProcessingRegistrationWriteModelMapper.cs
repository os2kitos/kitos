using System.Linq;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GDPR.Write;
using Infrastructure.Services.Types;
using Presentation.Web.Models.API.V2.Request.DataProcessing;

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
            return Map(dto);
        }
        private DataProcessingRegistrationModificationParameters Map(DataProcessingRegistrationWriteRequestDTO dto)
        {
            return new DataProcessingRegistrationModificationParameters
            {
                Name = dto.Name.AsChangedValue(),
                General = dto.General.FromNullable().Select(MapGeneral)
            };
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
                HasSubDataProcesors = (dto.HasSubDataProcesors?.ToYesNoUndecidedOption()).AsChangedValue(),
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
    }
}