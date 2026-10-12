using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1
{
    public class UserCredentialsDTO
    {
        [Required(AllowEmptyStrings = false)]
        public required string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        public required string Password { get; set; }
    }
}