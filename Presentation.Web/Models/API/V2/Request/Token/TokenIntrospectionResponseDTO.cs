using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.Token
{
    public class TokenIntrospectionResponseDTO
    {
        public bool Active { get; set; }
        public DateTime Expiration { get; set; }
        public IEnumerable<ClaimResponseDTO> Claims { get; set; }
    }
}