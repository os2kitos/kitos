using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Municipality_Id { get; set; }
        public string Role { get; set; }
    }
}