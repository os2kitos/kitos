using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V1.Mapping;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationChangeLogResponseDTO
    {
        public StsOrganizationChangeLogOriginOption Origin { get; set; }
        public string? Name { get; set; }
        public DateTime LogTime { get; set; }
        public IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> Consequences { get; set; }
    }
}