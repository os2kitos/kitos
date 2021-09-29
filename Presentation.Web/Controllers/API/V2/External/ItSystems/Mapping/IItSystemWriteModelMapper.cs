using Core.ApplicationServices.Model.System;
using Presentation.Web.Models.API.V2.Request.System;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public interface IItSystemWriteModelMapper
    {
        RightsHolderSystemCreationParameters FromRightsHolderPOST(RightsHolderCreateItSystemRequestDTO dto);
        RightsHolderSystemUpdateParameters FromRightsHolderPUT(RightsHolderWritableITSystemPropertiesDTO dto);
        RightsHolderSystemUpdateParameters FromRightsHolderPATCH(RightsHolderPartialUpdateSystemPropertiesRequestDTO dto);
    }

}
