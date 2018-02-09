using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemUsageDataWorkerRelationDTO 
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; }
        public int LastChangedByUserId { get; set; }
        public int ObjectOwnerId { get; set; }
        public int DataWorkerId { get; set; }
        public string DataWorkerName { get; set; }
        public string DataWorkerCvr { get; set; }

    }
}
