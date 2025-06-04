using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class PasswordResetResponseDTO
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}