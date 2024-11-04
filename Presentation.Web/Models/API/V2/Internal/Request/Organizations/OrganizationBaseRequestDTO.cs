using Presentation.Web.Models.API.V2.Types.Organization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationBaseRequestDTO
    {
        public string Name { get; set; }
        public OrganizationType Type { get; set; }
        [MaxLength(10)]
        public string Cvr {  get; set; }
        public string ForeignCvr { get; set; }
    }
}