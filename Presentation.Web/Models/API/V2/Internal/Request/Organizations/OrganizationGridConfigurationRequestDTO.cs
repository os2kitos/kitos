using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationGridConfigurationRequestDTO
    {
      
        public IEnumerable<ColumnConfigurationRequestDTO> VisibleColumns { get; set; }
    }
}
