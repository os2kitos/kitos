using System;
using System.Security.Claims;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Infrastructure
{
    public class TokenValidator
    {
        public ILogger Logger = Log.Logger;

        public KitosApiToken CreateToken(Core.DomainModel.User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var handler = new JwtSecurityTokenHandler();

            var identity = new ClaimsIdentity(new GenericIdentity(user.Id.ToString(), "TokenAuth"));

            // securityKey length should be >256b
            try
            {
                var validFrom = DateTime.UtcNow;
                var expires = validFrom.AddDays(1);
                var securityToken = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = identity,
                    Issuer = BearerTokenConfig.Issuer,
                    IssuedAt = validFrom,
                    Expires = expires,
                    SigningCredentials = new SigningCredentials(BearerTokenConfig.SecurityKey, SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest)
                });
                var tokenString = handler.WriteToken(securityToken);
                return new KitosApiToken(user, tokenString, expires);
            }
            catch (Exception exn)
            {
                Logger.Error(exn, "TokenValidator: Exception creating token.");
                throw;
            }
        }
    }
}