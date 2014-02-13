using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email adresse")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Forbliv logget ind?")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Display(Name = "Email adresse")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string ResetHash { get; set; }

        public string Email { get; set; }

        [Required]
        [Display(Name = "Nyt password")]
        public string Password { get; set; }

        [Compare("Password")]
        [Display(Name = "Bekræft nyt password")]
        public string ConfirmPassword { get; set; }
    }
}