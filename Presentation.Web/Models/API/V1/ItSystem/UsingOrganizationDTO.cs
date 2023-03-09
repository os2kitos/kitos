using System;

namespace Presentation.Web.Models.API.V1.ItSystem
{
    public class UsingOrganizationDTO
    {
        public Guid SystemUsageUuid { get; set; }
        public NamedEntityDTO Organization { get; set; }
        public Guid OrganizationUuid { get; set; }
    }
}