using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class ArchivePeriodDTO
    {
        public int Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UniqueArchiveId { get; set; }
        public int ItSystem_Id { get; set; }
    }
}