using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationGridConfigurationRequestDTO
    {
      
        public IEnumerable<ColumnConfigurationRequestDTO> VisibleColumns { get; set; }
    }
}
