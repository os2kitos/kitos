using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationConnectionAddNewLogsResult
    {
        public StsOrganizationConnectionAddNewLogsResult(IEnumerable<StsOrganizationChangeLog> removedChangeLogs)
        {
            RemovedChangeLogs = removedChangeLogs ?? new List<StsOrganizationChangeLog>();
        }

        public IEnumerable<StsOrganizationChangeLog> RemovedChangeLogs { get; }
    }
}
