using Core.ApplicationServices.Model.GDPR.Write;
using Presentation.Web.Models.API.V2.Request.DataProcessing;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public interface IDataProcessingRegistrationWriteModelMapper
    {
        DataProcessingRegistrationModificationParameters FromPOST(CreateDataProcessingRegistrationRequestDTO dto);
        DataProcessingRegistrationModificationParameters FromPUT(UpdateDataProcessingRegistrationRequestDTO dto);
        DataProcessingRegistrationModificationParameters FromPATCH(UpdateDataProcessingRegistrationRequestDTO dto);
    }
}
