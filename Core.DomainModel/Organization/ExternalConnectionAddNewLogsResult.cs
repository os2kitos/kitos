using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class ExternalConnectionAddNewLogsResult
    {
        public ExternalConnectionAddNewLogsResult(IEnumerable<IExternalConnectionChangelog> removedChangeLogs)
        {
            RemovedChangeLogs = removedChangeLogs ?? new List<IExternalConnectionChangelog>();
        }

        public IEnumerable<IExternalConnectionChangelog> RemovedChangeLogs { get; }
    }
}
