using Core.ApplicationServices.Model.Interface;
using Presentation.Web.Models.API.V2.Request.Interface;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping
{
    public interface IItInterfaceWriteModelMapper
    {
        RightsHolderItInterfaceCreationParameters FromPOST(RightsHolderCreateItInterfaceRequestDTO dto);
        RightsHolderItInterfaceUpdateParameters FromPATCH(RightsHolderPartialUpdateItInterfaceRequestDTO dto);
        RightsHolderItInterfaceUpdateParameters FromPUT(RightsHolderWritableItInterfacePropertiesDTO dto);
        ItInterfaceWriteModel FromPOST(CreateItInterfaceRequestDTO request);
        ItInterfaceWriteModel FromPATCH(UpdateItInterfaceRequestDTO request);
    }
}
