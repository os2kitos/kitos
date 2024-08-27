using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class OrganizationGridConfigurationResponseDTO
    {
        public Guid OrganizationUuid { get; set; }
        public OverviewType OverviewType { get; set; }
        public string Version { get; set; }
        public IEnumerable<ColumnConfigurationResponseDTO> VisibleColumns { get; set; }
    }
}