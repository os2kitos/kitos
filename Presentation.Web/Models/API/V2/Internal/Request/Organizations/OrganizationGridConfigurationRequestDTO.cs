using Core.DomainModel;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class OrganizationGridConfigurationRequestDTO
    {
        public Guid OrganizationUuid { get; }
        public OverviewType OverviewType { get; }
        public string Version { get; }
        public IEnumerable<KendoColumnConfigurationDTO> VisibleColumns { get; }
    }
}
