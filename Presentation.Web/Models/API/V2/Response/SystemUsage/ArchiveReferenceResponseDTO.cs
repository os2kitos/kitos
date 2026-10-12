using System;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class ArchiveReferenceResponseDTO : SimpleLinkDTO
    {
        public Guid Uuid { get; set; }
    }
}
