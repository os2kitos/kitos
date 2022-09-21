using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V2.Types.SystemUsage
{
    public enum LifeCycleStatusChoice
    {
        Undecided = 0,
        NotInUse = 1,
        PhasingIn = 2,
        Operational = 3,
        PhasingOut = 4
    }
}