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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Presentation.Web.Infrastructure
{
    public class TokenValidator
    {
        public ClaimsPrincipal Validate(string idToken)
        {
            try
            {
                var ssoConfig = GetKeyFromConfig();
                if (ssoConfig == null)
                {
                    //TODO log 
                    return null;
                }
                var tokenhandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
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
                //TODO log exception
                return null;
            }
        }

        private SsoConfig GetKeyFromConfig()
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
                    result.SigningKey = new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(jwks)));

                }
            }
            catch (Exception)
            {
                //TODO log exception
                return null;
            }
            return result;
        }
    }

    internal class SsoConfig
    {
        public SecurityKey SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}