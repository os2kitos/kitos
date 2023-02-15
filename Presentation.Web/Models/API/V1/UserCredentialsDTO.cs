using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1
{
    public class UserCredentialsDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}