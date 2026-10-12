using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Authentication;
using Core.DomainModel;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Core.ApplicationServices.Authentication
{
    public class TokenValidator : ITokenValidator
    {
        public ILogger Logger = Log.Logger;
        private readonly string _baseUrl;
        private readonly SecurityKey _signingKey;

        public TokenValidator(string baseUrl, SecurityKey signingKey)
        {
            _baseUrl = baseUrl;
            _signingKey = signingKey ?? throw new ArgumentNullException(nameof(signingKey));
        }

        public KitosApiToken CreateToken(DomainModel.User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var identity = new ClaimsIdentity(new GenericIdentity(user.Id.ToString(), "TokenAuth"));
            AddUserClaims(identity, user);

            // securityKey length should be >256b
            try
            {
                var validFrom = DateTime.UtcNow;
                var expires = validFrom.AddDays(1);

                var handler = new JsonWebTokenHandler();
                var tokenString = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = identity,
                    Issuer = _baseUrl,
                    IssuedAt = validFrom,
                    Expires = expires,
                    SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
                });

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
            var handler = new JsonWebTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = true,
                ValidIssuer = _baseUrl,
                ValidateLifetime = true,
                ValidateAudience = false
            };

            try
            {
                var result = handler.ValidateTokenAsync(token, validationParams).GetAwaiter().GetResult();

                if (!result.IsValid)
                {
                    Logger.Error(result.Exception, "TokenValidator: Token validation failed.");
                    return new OperationError("Invalid token", OperationFailure.Forbidden);
                }

                var jwtToken = (JsonWebToken)result.SecurityToken;

                return new TokenIntrospectionResponse
                {
                    Active = true,
                    Expiration = jwtToken.ValidTo,
                    Claims = result.ClaimsIdentity.Claims
                        .Select(c => new ClaimResponse { Type = c.Type, Value = c.Value })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "TokenValidator: Exception verifying token.");
                return new OperationError("Invalid token", OperationFailure.Forbidden);
            }
        }

        private void AddUserClaims(ClaimsIdentity identity, User user)
        {
            if (user.IsGlobalAdmin)
            {
                identity.AddClaim(new Claim(Constants.ClaimConstants.CanPublish, Constants.ClaimConstants.True));
            }

            if (user.IsSystemIntegrator)
            {
                identity.AddClaim(new Claim(Constants.ClaimConstants.CanSubscribe, Constants.ClaimConstants.True));
            }
        }
    }
}
