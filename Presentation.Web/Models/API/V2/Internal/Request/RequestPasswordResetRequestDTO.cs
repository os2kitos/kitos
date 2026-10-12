using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request
{
    public class RequestPasswordResetRequestDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
    }
}