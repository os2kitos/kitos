using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class UserOverviewDTO : UserDTO
    {
        public bool CanBeEdited { get; set; }
    }
}
