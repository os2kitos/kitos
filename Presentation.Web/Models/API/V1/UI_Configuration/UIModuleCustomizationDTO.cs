using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.UI_Configuration
{
    public class UIModuleCustomizationDTO
    {
        public IEnumerable<CustomizedUINodeDTO> Nodes { set; get; }
    }
}