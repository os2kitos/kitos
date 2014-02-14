using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        [HiddenInput(DisplayValue = false)]
        public string RequestHash { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Nyt password")]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Passwordet skal være mindst 6 tegn")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Password")]
        [Display(Name = "Bekræft nyt password")]
        public string ConfirmPassword { get; set; }
    }
}