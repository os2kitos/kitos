using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Request.User
{
    public class CreateUserRequestDTO  : BaseUserRequestDTO
    {
        [Required]
        [EmailAddress]
        public new required string Email { get; set; }

        [Required]
        public new required string FirstName { get; set; }

        [Required]
        public new required string LastName { get; set; }

    }
}