using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.Organization
{
    public enum ConnectionUpdateOrganizationUnitChangeType
    {
        Added = 0,
        Renamed = 1,
        Moved = 2,
        Deleted = 3,
        Converted = 4
    }
}
