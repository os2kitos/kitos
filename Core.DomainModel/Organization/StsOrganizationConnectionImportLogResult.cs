using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationConnectionImportLogResult
    {
        public StsOrganizationConnectionImportLogResult(IEnumerable<StsOrganizationChangeLog> addedChangeLogs, IEnumerable<StsOrganizationChangeLog> removedChangeLogs)
        {
            AddedChangeLogs = addedChangeLogs;
            RemovedChangeLogs = removedChangeLogs;
        }

        public IEnumerable<StsOrganizationChangeLog> AddedChangeLogs { get; }
        public IEnumerable<StsOrganizationChangeLog> RemovedChangeLogs { get; }
    }
}
