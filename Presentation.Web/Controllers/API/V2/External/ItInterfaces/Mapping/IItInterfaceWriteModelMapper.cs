using Core.ApplicationServices.Model.Interface;
using Presentation.Web.Models.API.V2.Request.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping
{
    public interface IItInterfaceWriteModelMapper
    {
        RightsHolderItInterfaceCreationParameters FromPOST(RightsHolderCreateItInterfaceRequestDTO dto);
        RightsHolderItInterfaceUpdateParameters FromPATCH(RightsHolderWritableItInterfacePropertiesDTO dto);
        RightsHolderItInterfaceUpdateParameters FromPUT(RightsHolderWritableItInterfacePropertiesDTO dto);
    }
}
