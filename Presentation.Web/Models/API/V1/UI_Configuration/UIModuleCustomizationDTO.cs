using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.UI_Configuration
{
    public class UIModuleCustomizationDTO
    {
        public List<CustomizedUINodeDTO> Nodes { set; get; }
    }
}