using Core.ApplicationServices.Model.GDPR.Write;
using Presentation.Web.Models.API.V2.Request.DataProcessing;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public interface IDataProcessingRegistrationWriteModelMapper
    {
        DataProcessingRegistrationModificationParameters FromPOST(DataProcessingRegistrationWriteRequestDTO dto);
        DataProcessingRegistrationModificationParameters FromPUT(DataProcessingRegistrationWriteRequestDTO dto);
        UpdatedDataProcessingRegistrationGeneralDataParameters MapGeneral(DataProcessingRegistrationGeneralDataWriteRequestDTO dto);
    }
}
