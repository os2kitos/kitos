using Core.DomainModel.ItSystem;
using Presentation.Web.Models.API.V2.Response.Interface;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping
{
    public interface IItInterfaceResponseMapper
    {
        ItInterfaceResponseDTO ToItInterfaceResponseDTO(ItInterface itInterface);
        RightsHolderItInterfaceResponseDTO ToRightsHolderItInterfaceResponseDTO(ItInterface itInterface);
    }
}
