using System;
using System.Security.Claims;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Presentation.Web.Infrastructure.Model.Authentication;
using Core.Abstractions.Types;
using System.Linq;

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

        public Result<TokenIntrospectionResponse, OperationError> VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = BearerTokenConfig.SecurityKey,
                ValidateIssuer = true,
                ValidIssuer = BearerTokenConfig.Issuer,
                ValidateLifetime = true,
                ValidateAudience = false
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParams, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                return new TokenIntrospectionResponse
                {
                    Active = true,
                    Expiration = jwtToken.ValidTo,
                    Claims = principal.Claims.Select(c => new ClaimResponse { Type = c.Type, Value = c.Value }).ToList()
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "TokenValidator: Exception verifying token.");
                return new OperationError("Invalid token", OperationFailure.Forbidden);
            }
        }
    }
}