using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presentation.Web.Models.Shared;

namespace Presentation.Web.Models.GDPR
{
    public class DataProcessingOptionsDTO
    {
        public IEnumerable<BusinessRoleDTO> roles { get; set; }
        public IEnumerable<SimpleOptionDTO> dataResponsibleOptions { get; set; }
    }
}