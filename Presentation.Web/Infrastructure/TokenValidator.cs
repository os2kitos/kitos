using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using Presentation.Web.Infrastructure.Model;
using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Infrastructure
{
    public class TokenValidator
    {
        public ILogger Logger = Log.Logger;
        public ClaimsPrincipal Validate(string idToken)
        {
            try
            {
                var ssoConfig = GetKeyFromConfig();
                if (ssoConfig == null)
                {
                    Logger.Error("TokenValidator: Could not load SSOConfig");
                    return null;
                }
                var tokenhandler = new JwtSecurityTokenHandler();
                SecurityToken sToken;
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = ssoConfig.Audience,
                    ValidIssuer = ssoConfig.Issuer,
                    IssuerSigningKey = ssoConfig.SigningKey
                };
                return tokenhandler.ValidateToken(idToken, tokenValidationParameters, out sToken);
            }
            catch (Exception e)
            {
                Logger.Error(e, "TokenValidator: Error validating token");
                return null;
            }
        }

        public KitosApiToken CreateToken(Core.DomainModel.User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var handler = new JwtSecurityTokenHandler();

            var identity = new ClaimsIdentity(new GenericIdentity(user.Id.ToString(), "TokenAuth"));
            if (user.DefaultOrganizationId.HasValue)
            {
                identity.AddClaim(new Claim("DefaultOrganization", user.DefaultOrganizationId.Value.ToString("D")));
            }

            // securityKey length should be >256b
            try
            {
                var validFrom = DateTime.UtcNow;
                var expires = validFrom.AddDays(1);
                var securityToken = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = identity,
                    TokenIssuerName = BearerTokenConfig.Issuer,
                    Lifetime = new System.IdentityModel.Protocols.WSTrust.Lifetime(validFrom, expires),
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

        public SsoConfig GetKeyFromConfig()
        {
            var result = new SsoConfig();
            var configUrl = ConfigurationManager.AppSettings["SSOGateway"];
            result.Audience = ConfigurationManager.AppSettings["SSOAudience"];
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadString(configUrl);
                    var openidConfig = JsonConvert.DeserializeObject<dynamic>(json);
                    result.Issuer = openidConfig.issuer;

                    var jwksuri = (string)openidConfig.jwks_uri;
                    var jwksjson = wc.DownloadString(jwksuri);
                    var jwks = JsonConvert.DeserializeObject<dynamic>(jwksjson);
                    var keys = (JArray)jwks.keys;
                    var cert = keys.First.Single(t => t.Path.Contains("x5c")).First.First.ToString();
                    result.SigningKey = new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(cert)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "TokenValidator: Exception while getting Signingkey from " + configUrl);
                return null;
            }
            return result;
        }
    }
}