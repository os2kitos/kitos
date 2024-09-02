using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationGridConfigurationRequestDTO
    {
        public Guid OrganizationUuid { get; set; }
        public OverviewType OverviewType { get; set; }
        public string Version { get; set; }
        public IEnumerable<ColumnConfigurationRequestDTO> VisibleColumns { get; set; }
    }
}
