using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class UIModuleCustomizationResponseDTO
    {

        public string Module { get; set; }

        public IList<CustomizedUINodeResponseDTO> Nodes { get; set; }
    }
}