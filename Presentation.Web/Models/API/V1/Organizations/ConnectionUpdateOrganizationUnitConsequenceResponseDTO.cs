using System;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ConnectionUpdateOrganizationUnitConsequenceResponseDTO
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public ConnectionUpdateOrganizationUnitChangeCategory Category { get; set; }
        public string Description { get; set; }
    }
}