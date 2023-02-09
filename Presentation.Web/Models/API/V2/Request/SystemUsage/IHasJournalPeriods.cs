using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public interface IHasJournalPeriods<T> where T: JournalPeriodDTO
    {
        public IEnumerable<T> JournalPeriods { get; set; }
    }
}
