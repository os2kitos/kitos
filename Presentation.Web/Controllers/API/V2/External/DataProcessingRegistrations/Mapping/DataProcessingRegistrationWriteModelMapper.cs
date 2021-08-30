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

        private UpdatedDataProcessingRegistrationGeneralDataParameters MapGeneral(DataProcessingRegistrationGeneralDataWriteRequestDTO dto)
        {
            throw new System.NotImplementedException();
        }
    }
}