using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request
{
    public class RequestPasswordResetRequestDTO
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}