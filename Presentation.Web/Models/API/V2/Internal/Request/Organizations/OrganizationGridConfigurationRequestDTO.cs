using Core.DomainModel;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class OrganizationGridConfigurationRequestDTO
    {
        public Guid OrganizationUuid { get; set; }
        public OverviewType OverviewType { get; set; }
        public string Version { get; set; }
        public IEnumerable<KendoColumnConfigurationDTO> VisibleColumns { get; set; }
    }
}
