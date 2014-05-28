using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class RightOutputDTO
    {
        public int ObjectId { get; set; }

        public int RoleId { get; set; }

        public int UserId { get; set; }
        public UserDTO User { get; set; }
    }
}