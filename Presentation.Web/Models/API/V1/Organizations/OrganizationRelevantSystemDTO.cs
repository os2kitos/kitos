using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class OrganizationRelevantSystemDTO
    {
        public int SystemId { get; set; }
        public IEnumerable<int> RelevantUnitIds { get; set; }
    }
}