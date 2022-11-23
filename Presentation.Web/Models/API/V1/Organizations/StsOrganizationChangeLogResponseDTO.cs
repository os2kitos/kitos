using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationChangeLogResponseDTO
    {
        public StsOrganizationChangeLogOriginOption Origin { get; set; }
        public UserWithEmailDTO User { get; set; }
        public DateTime LogTime { get; set; }
        public IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> Consequences { get; set; }
    }
}