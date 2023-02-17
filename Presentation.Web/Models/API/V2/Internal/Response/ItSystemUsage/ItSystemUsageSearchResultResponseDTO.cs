using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageSearchResultResponseDTO : IHasUuidExternal
    {
        [Required]
        public Guid Uuid { get; set; }
        [Required]
        public bool Valid { get; set; }
        [Required]
        public ItSystemUsageSystemContextResponseDTO SystemContext { get; set; }

        public ItSystemUsageSearchResultResponseDTO()
        {
        }

        public ItSystemUsageSearchResultResponseDTO(Guid uuid, bool valid, ItSystemUsageSystemContextResponseDTO systemContext)
        {
            Uuid = uuid;
            Valid = valid;
            SystemContext = systemContext;
        }
    }
}