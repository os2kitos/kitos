using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class ItSystemUsageMigrationDTO
    {
        public int ItSystemUsageId { get; set; }
        public int ItSystemUsageName { get; set; }

        public int FromItSystemId { get; set; }
        public int FromItSystemName { get; set; }

        public int ToItSystemId { get; set; }
        public int ToItSystemName { get; set; }
    }
}