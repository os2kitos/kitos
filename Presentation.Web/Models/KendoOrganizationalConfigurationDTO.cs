using Core.DomainModel;

namespace Presentation.Web.Models
{
    public class KendoOrganizationalConfigurationDTO
    {
        public int OrganizationId { get; set; }
        public OverviewType OverviewType { get; set; }
        public string Configuration { get; set; }
    }
}