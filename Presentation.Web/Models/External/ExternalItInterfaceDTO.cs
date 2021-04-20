using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External
{
    public class ExternalItInterfaceDTO
    {
        [Required]
        public Guid Uuid { get; set; }
    }
}