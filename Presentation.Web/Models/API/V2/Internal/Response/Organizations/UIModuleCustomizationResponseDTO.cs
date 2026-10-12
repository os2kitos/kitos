using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class UIModuleCustomizationResponseDTO
    {

        public required string Module { get; set; }

        public required IEnumerable<CustomizedUINodeResponseDTO> Nodes { get; set; }
    }
}