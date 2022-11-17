using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationChangelogDTO
    {
        public string ResponsibleEntityName { get; set; }
        public string OrganizationUnit { get; set; }
        public IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> Consequences { get; set; }
    }
}