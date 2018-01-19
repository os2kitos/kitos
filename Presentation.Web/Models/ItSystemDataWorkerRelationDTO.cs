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
        public int DataWorkerId { get; set; }
        public string DataWorkerName { get; set; }
        public string DataWorkerCvr { get; set; }
        
    }
}