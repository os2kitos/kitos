using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel.Organization
{
    public class DisconnectOrganizationFromOriginResult
    {
        public IEnumerable<OrganizationUnit> ConvertedUnits { get; }
        public DisconnectOrganizationFromOriginResult(IEnumerable<OrganizationUnit> convertedUnits)
        {
            ConvertedUnits = convertedUnits.ToList().AsReadOnly();
        }
    }
}
