using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class ItSystemDataWorkerRelationDTO
    {
        public int Id { get; set; }
        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public int DataWorkerId { get; set; }
        public virtual Organization DataWorker { get; set; }
    }
}