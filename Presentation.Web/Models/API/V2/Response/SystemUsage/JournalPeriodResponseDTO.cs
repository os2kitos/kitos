using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class JournalPeriodResponseDTO : JournalPeriodDTO, IHasUuidExternal
    {
        [Required]
        public Guid Uuid { get; set; }
    }
}