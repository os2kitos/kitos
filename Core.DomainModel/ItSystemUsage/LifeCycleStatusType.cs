using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystemUsage
{
    public enum LifeCycleStatusType
    {
        Undecided = 0,
        NotInUse = 1,
        PhasingIn = 2,
        Operational = 3,
        PhasingOut = 4
    }
}
