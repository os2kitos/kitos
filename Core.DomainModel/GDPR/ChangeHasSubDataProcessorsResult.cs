using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.GDPR
{
    public class ChangeHasSubDataProcessorsResult
    {
        public IEnumerable<SubDataProcessor> RemovedSubDataProcessors { get; }

        public ChangeHasSubDataProcessorsResult(IEnumerable<SubDataProcessor> removedSubDataProcessors)
        {
            RemovedSubDataProcessors = removedSubDataProcessors?.ToList() ?? new List<SubDataProcessor>();
        }
    }
}
