using Core.DomainModel;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class KendoOrganizationalConfigurationDTO
    {
        public int OrganizationId { get; set; }
        public OverviewType OverviewType { get; set; }
        public string Version { get; set; }
        public IEnumerable<KendoColumnConfigurationDTO> Columns { get; set; }
    }

    public class KendoColumnConfigurationDTO
    {
        public string PersistId { get; set; }
        public int Index { get; set; }
        public bool Hidden { get; set; }
    }
}