using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GDPR.Write;
using Presentation.Web.Models.API.V2.Request.DataProcessing;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public class DataProcessingRegistrationWriteModelMapper : IDataProcessingRegistrationWriteModelMapper
    {
        public DataProcessingRegistrationModificationParameters FromPOST(DataProcessingRegistrationWriteRequestDTO dto)
        {
            return new DataProcessingRegistrationModificationParameters
            {
                Name = dto.Name.AsChangedValue()
            };
        }

        public DataProcessingRegistrationModificationParameters FromPUT(DataProcessingRegistrationWriteRequestDTO dto)
        {
            return new DataProcessingRegistrationModificationParameters
            {
                Name = dto.Name.AsChangedValue()
            };
        }
    }
}