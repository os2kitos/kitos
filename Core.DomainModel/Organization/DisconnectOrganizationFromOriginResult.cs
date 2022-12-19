using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.Organization
{
    public class DisconnectOrganizationFromOriginResult
    {
        public DisconnectOrganizationFromOriginResult(IEnumerable<OrganizationUnit> convertedUnits, IEnumerable<StsOrganizationChangeLog> removedChangeLogs)
        {
            ConvertedUnits = convertedUnits.ToList().AsReadOnly();
            RemovedChangeLogs = removedChangeLogs;
        }

        public IEnumerable<OrganizationUnit> ConvertedUnits { get; }
        public IEnumerable<StsOrganizationChangeLog> RemovedChangeLogs { get; }
    }
}
