using Core.ApplicationServices.Model.GDPR.Write;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public interface IDataProcessingRegistrationWriteModelMapper
    {
        DataProcessingRegistrationCreationParameters FromPOST(CreateDataProcessingRegistrationRequestDTO dto);
        DataProcessingRegistrationModificationParameters FromPUT(UpdateDataProcessingRegistrationRequestDTO dto);
        DataProcessingRegistrationModificationParameters FromPATCH(UpdateDataProcessingRegistrationRequestDTO dto);
        UpdatedDataProcessingRegistrationOversightDateParameters FromOversightPOST(CreateOversightDateDTO dto);
        UpdatedDataProcessingRegistrationOversightDateParameters FromOversightPATCH(ModifyOversightDateDTO dto);
    }
}
