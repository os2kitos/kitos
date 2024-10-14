using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class UIModuleCustomizationRequestDTO
    {
        [Required]
        public IEnumerable<CustomizedUINodeRequestDTO> Nodes { set; get; }
    }
}