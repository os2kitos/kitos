using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccess.Mapping
{
    using Core.DomainModel.ItSystemUsage;

    using ArchiveLocation = Core.DomainModel.ItSystem.ArchiveLocation;

    public class ArchiveLocationMap : OptionEntityMap<ArchiveLocation, ItSystemUsage>
    {
    }
}
