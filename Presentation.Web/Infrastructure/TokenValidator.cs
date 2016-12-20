using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Microsoft.IdentityModel.Tokens;

namespace Presentation.Web.Infrastructure
{
    public class TokenValidator
    {
        public ClaimsPrincipal Validate(string idToken)
        {
            try
            {
                X509Certificate2 cert;
                using (var stream = new FileStream(@"C:\temp\syddjurs.pfx", FileMode.Open))
                {
                    var buffer = new byte[16*1024];
                    using (var ms = new MemoryStream())
                    {
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }

                        cert = new X509Certificate2(ms.ToArray(), "Miracle42");
                    }
                }

                var tokenhandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                SecurityToken sToken;
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = "memaclient",
                    ValidIssuer = "https://os2sso-test.miracle.dk",
                    IssuerSigningKey = new X509SecurityKey(cert)
                };
                return tokenhandler.ValidateToken(idToken, tokenValidationParameters, out sToken);
            }
            catch (Exception e)
            {
                //TODO log exception
                return null;
            }
        }
    }
}