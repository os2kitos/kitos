using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageSearchResultResponseDTO : IHasUuidExternal
    {
        [Required]
        public required Guid Uuid { get; set; }
        [Required]
        public required bool Valid { get; set; }
        [Required]
        public required ItSystemUsageSystemContextResponseDTO SystemContext { get; set; }

        public ItSystemUsageSearchResultResponseDTO()
        {
        }

        [SetsRequiredMembers]
        public ItSystemUsageSearchResultResponseDTO(Guid uuid, bool valid, ItSystemUsageSystemContextResponseDTO systemContext)
        {
            Uuid = uuid;
            Valid = valid;
            SystemContext = systemContext;
        }
    }
}