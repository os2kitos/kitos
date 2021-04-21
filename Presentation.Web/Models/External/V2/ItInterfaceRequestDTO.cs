using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2
{
    public class ItInterfaceRequestDTO
    {
        [Required]
        public Guid Uuid { get; set; }
        [Required] 
        public Guid ExposedBySystemUuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string InterfaceId { get; set; }
        public string Version { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string UrlReference { get; set; }
    }
}