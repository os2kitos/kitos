using Core.ApplicationServices.Model.System;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public interface IItSystemWriteModelMapper
    {
        SystemCreationParameters FromRightsHolderPOST(RightsHolderFullItSystemRequestDTO dto);
        SystemUpdateParameters FromRightsHolderPUT(RightsHolderFullItSystemRequestDTO dto);
        SystemUpdateParameters FromRightsHolderPATCH(RightsHolderUpdateSystemPropertiesRequestDTO dto);
    }

}
