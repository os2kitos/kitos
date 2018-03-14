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

        public string CreateToken(Core.DomainModel.User user) {

            var ssoConfig = GetKeyFromConfig();

            var handler = new JwtSecurityTokenHandler();

            ClaimsIdentity identity = new ClaimsIdentity(
                new GenericIdentity(user.Id.ToString(), "TokenAuth")/*,
                new[] {
                }*/
            );
            string key = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";

            var test = ssoConfig.SigningKey;
           // Create Security key  using private key above:
           /* // not that latest version of JWT using Microsoft namespace instead of System */
           var securityKey = new System.IdentityModel.Tokens.InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
              
            // Also note that securityKey length should be >256b
            // so you have to make sure that your private key has a proper length
            //
            try { 
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                //Issuer = ssoConfig.Issuer,
                //Audience = ssoConfig.Audience,
                Subject = identity,
                TokenIssuerName = ssoConfig.Issuer,
                Lifetime = new System.IdentityModel.Protocols.WSTrust.Lifetime(DateTime.Now, DateTime.Now.AddDays(1)),
                
                
              //  Expires = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1),
                SigningCredentials = new System.IdentityModel.Tokens.SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest)// SecurityAlgorithms.DefaultDigestAlgorithm) // SecurityAlgorithms.//.RsaV15KeyWrap)//HmacSha256Signature)
                
            });


                return handler.WriteToken(securityToken);
            }
            catch(Exception E) { }
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