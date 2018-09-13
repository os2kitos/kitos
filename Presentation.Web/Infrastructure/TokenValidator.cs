using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using System.Text;

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
                var tokenhandler = new System.IdentityModel.Tokens.JwtSecurityTokenHandler();
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

        public string CreateToken(Core.DomainModel.User user)
        {

            var ssoConfig = GetKeyFromConfig();

            var handler = new JwtSecurityTokenHandler();

            ClaimsIdentity identity = new ClaimsIdentity(
                new GenericIdentity(user.Id.ToString(), "TokenAuth")/*,
                Add claims here:
                new[] {
                }*/
            );

            string key = System.Web.Configuration.WebConfigurationManager.AppSettings["SecurityKeyString"];

            // Create Security key  using private key above:
            var securityKey = new System.IdentityModel.Tokens.InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // securityKey length should be >256b
            try
            {
                var securityToken = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = identity,
                    TokenIssuerName = ssoConfig.Issuer,
                    Lifetime = new System.IdentityModel.Protocols.WSTrust.Lifetime(DateTime.Now, DateTime.Now.AddDays(1)),
                    SigningCredentials = new System.IdentityModel.Tokens.SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest)
                });
                return handler.WriteToken(securityToken);
            }
            catch (Exception E)
            {
                Logger.Error("TokenValidator: Exception creating token. Message: " + E.Message);
                return null;
            }
            return null;
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

                    var jwksuri = (string) openidConfig.jwks_uri;
                    var jwksjson = wc.DownloadString(jwksuri);
                    var jwks = JsonConvert.DeserializeObject<dynamic>(jwksjson);
                    var keys = (JArray) jwks.keys;
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

    public class SsoConfig
    {
        public SecurityKey SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}