using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.UI_Configuration
{
    public class UIModuleCustomizationDTO
    {
        [Required]
        public IEnumerable<CustomizedUINodeDTO> Nodes { set; get; }
    }
}