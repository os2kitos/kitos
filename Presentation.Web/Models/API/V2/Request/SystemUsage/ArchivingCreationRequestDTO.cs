using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class ArchivingCreationRequestDTO : BaseArchivingWriteRequestDTO, IHasJournalPeriods<JournalPeriodDTO>
    {
        public IEnumerable<JournalPeriodDTO> JournalPeriods { get; set; }
    }
}