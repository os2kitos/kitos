using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Presentation.Web.Models.API.V1
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Dette felt skal udfyldes")]
        [EmailAddress(ErrorMessage = "Dette felt er ikke en gyldig email-addresse")]
        [Display(Name = "Email adresse")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Dette felt skal udfyldes")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Forbliv logget ind?")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Dette felt skal udfyldes")]
        [EmailAddress(ErrorMessage = "Dette felt er ikke en gyldig email-addresse")]
        [Display(Name = "Email adresse")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public string RequestHash { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Email adresse")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Dette felt skal udfyldes")]
        [Display(Name = "Nyt password")]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Passwordet skal være mindst 6 tegn")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Dette felt skal udfyldes")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "De to felter svarer ikke overens")]
        [Display(Name = "Bekræft nyt password")]
        public string ConfirmPassword { get; set; }
    }
}
