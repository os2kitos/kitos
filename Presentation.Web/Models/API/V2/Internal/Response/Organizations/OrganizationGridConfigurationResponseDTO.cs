using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class OrganizationGridConfigurationResponseDTO
    {
        public Guid OrganizationUuid { get; set; }
        public OverviewType OverviewType { get; set; }
        public IEnumerable<ColumnConfigurationResponseDTO> VisibleColumns { get; set; }
    }
}