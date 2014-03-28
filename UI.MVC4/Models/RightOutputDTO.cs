using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class RightOutputDTO
    {
        public int Object_Id { get; set; }
        public int Role_Id { get; set; }
        public int User_Id { get; set; }
        public UserDTO User { get; set; }
    }
}