using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request
{
    public class ResetPasswordRequestDTO
    {
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}