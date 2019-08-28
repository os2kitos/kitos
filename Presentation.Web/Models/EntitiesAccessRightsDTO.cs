using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class EntitiesAccessRightsDTO
    {
        public bool CanCreate { get; set; }
        public bool CanView { get; set; }
    }
}