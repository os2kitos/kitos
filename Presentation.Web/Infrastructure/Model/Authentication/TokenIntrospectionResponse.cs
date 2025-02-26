using System;
using System.Collections.Generic;

namespace Presentation.Web.Infrastructure.Model.Authentication
{
    public class TokenIntrospectionResponse
    {
        public bool Active { get; set; }
        public DateTime Expiration { get; set; }
        public IEnumerable<ClaimResponse> Claims { get; set; }
    }
}