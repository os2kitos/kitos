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
        public bool IsGlobalAdmin { get; set; }
        public List<AdminRightDTO> AdminRights { get; set; }
    }
}