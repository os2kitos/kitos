using Core.DomainModel.ItSystem;
using Presentation.Web.Models.API.V2.Response.System;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public interface IItSystemResponseMapper
    {
        RightsHolderItSystemResponseDTO ToRightsHolderResponseDTO(ItSystem itSystem);
        ItSystemResponseDTO ToSystemResponseDTO(ItSystem itSystem);
    }
}
