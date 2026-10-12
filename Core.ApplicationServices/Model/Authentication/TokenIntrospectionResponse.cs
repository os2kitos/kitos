using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Authentication
{
    public class TokenIntrospectionResponse
    {
        public bool Active { get; set; }
        public DateTime Expiration { get; set; }
        public required IEnumerable<ClaimResponse> Claims { get; set; }
    }
}