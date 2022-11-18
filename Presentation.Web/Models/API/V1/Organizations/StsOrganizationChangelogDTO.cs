using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationChangelogDTO
    {
        public StsOrganizationChangelogDTO(string responsibleEntityName, ConnectionUpdateOrganizationUnitConsequenceDTO consequence)
        {
            ResponsibleEntityName = responsibleEntityName;
            Consequence = consequence;
        }

        public string ResponsibleEntityName { get; set; }
        public ConnectionUpdateOrganizationUnitConsequenceDTO Consequence { get; set; }
    }
}