using Presentation.Web.Models.API.V2.Internal.Response.User;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class StsOrganizationChangeLogResponseDTO
    {
        public StsOrganizationChangeLogOriginOption Origin { get; set; }
        public UserReferenceResponseDTO User { get; set; }
        public DateTime LogTime { get; set; }
        public IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> Consequences { get; set; }
    }
}