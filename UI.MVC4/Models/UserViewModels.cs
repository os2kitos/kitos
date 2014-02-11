using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class UserViewModel
    {
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Brugernavn")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Forbliv logget ind?")]
        public bool RememberMe { get; set; }
        
    }
}