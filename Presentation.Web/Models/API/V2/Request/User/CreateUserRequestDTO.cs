using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Request.User
{
    public class CreateUserRequestDTO  : BaseUserRequestDTO
    {
        [Required]
        [EmailAddress]
        public new string Email { get; set; }

        [Required]
        public new string FirstName { get; set; }

        [Required]
        public new string LastName { get; set; }

    }
}