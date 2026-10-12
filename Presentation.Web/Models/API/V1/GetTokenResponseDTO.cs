using System;

namespace Presentation.Web.Models.API.V1
{
    public class GetTokenResponseDTO
    {
        public required string Token { get; set; }
        public required string Email { get; set; }
        public bool LoginSuccessful { get; set; }
        public DateTime Expires { get; set; }
    }
}