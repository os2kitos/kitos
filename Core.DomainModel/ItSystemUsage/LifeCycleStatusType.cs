using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystemUsage
{
    public enum LifeCycleStatusType
    {
        NotInUse = 0,
        PhasingIn = 1,
        Operational = 2,
        PhasingOut = 3
    }
}
