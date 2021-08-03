using Core.DomainModel;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class KendoOrganizationalConfigurationDTO
    {
        public int OrganizationId { get; set; }
        public OverviewType OverviewType { get; set; }
        public string VisibleColumnsCsv { get; set; }
        public int Version { get; set; }
    }
}