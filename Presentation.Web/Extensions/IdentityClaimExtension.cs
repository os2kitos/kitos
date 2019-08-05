using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Presentation.Web.Extensions
{
    public class IdentityClaimExtension
    {
        public Claim GetClaimOrNull(ClaimsIdentity claimHolder, string claimName)
        {
            return claimHolder.FindAll(x => x.Type == claimName).FirstOrDefault();
        }
    }
}