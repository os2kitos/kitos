using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2
{
    public class ItSystemRequestDTO
    {
        [Required]
        public Guid Uuid { get; set; }
        public Guid? ParentUuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string? FormerName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string UrlReference { get; set; }
        public Guid? BusinessTypeUuid { get; set; }
        public IEnumerable<string>? KLENumbers { get; set; }
        public IEnumerable<Guid>? KLEUuids { get; set; }
    }
}