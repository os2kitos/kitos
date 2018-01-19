using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemDataWorkerRelation: Entity
    {
        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public int DataWorkerId { get; set; }
        public virtual Organization.Organization DataWorker { get; set; } 

    }
}
