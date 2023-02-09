using System;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class JournalPeriodRequestDTO : JournalPeriodDTO
    {
        /// <summary>
        /// Optionally references an existing, specific journal period
        /// NOTE: If provided, an existing period with the same uuid must exist
        /// </summary>
        [NonEmptyGuid]
        public Guid? Uuid { get; set; }
    }
}