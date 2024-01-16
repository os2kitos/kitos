using System;
using System.Diagnostics;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.ServiceModel.Security;
using System.ServiceModel;
using System.Text;
using Infrastructure.STS.Common.Factories;

namespace Infrastructure.STS.Common.Model.Token
{
    public static class TokenFetcher
    {
        public static bool IsTokenExpired(SecurityToken token)
        {
            Debug.WriteLine("TokenId:" + token.Id);
            return token.ValidTo > DateTime.Now.AddMinutes(5);
        }

        public static SecurityToken IssueToken(string entityId, string cvr, string thumbprint, string endpoint, string issuer)
        {
            SecurityToken token = null;

            var certificate = X509CertificateClientCertificateFactory.GetClientCertificate(thumbprint);

            var absoluteUri = new Uri(entityId).AbsoluteUri;
            var cacheKey = new Guid(MD5.Create().ComputeHash(Encoding.Default.GetBytes(absoluteUri + "_" + cvr))).ToString();
            var inCache = CacheHelper.IsIncache(cacheKey);
            var needNewToken = false;

            if (inCache)
            {
                token = CacheHelper.GetFromCache<GenericXmlSecurityToken>(cacheKey);

                if (token.ValidTo.CompareTo(DateTime.Now) < 0)
                    needNewToken = true;
            }

            if (inCache && !needNewToken)
                return token;

            token = SendSecurityTokenRequest(absoluteUri, certificate, cvr, endpoint, issuer);
            CacheHelper.SaveToCache(cacheKey, token, token.ValidTo);

            return token;
        }

        private static SecurityToken SendSecurityTokenRequest(string appliesTo, X509Certificate2 clientCertificate, string cvr, string endpoint, string issuer)
        {
            var rst = new RequestSecurityToken
            {
                AppliesTo = new EndpointReference(appliesTo),
                RequestType = RequestTypes.Issue,
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                KeyType = KeyTypes.Asymmetric,
                Issuer = new EndpointReference(issuer),
                UseKey = new UseKey(new X509SecurityToken(clientCertificate)),
                Claims =
                {
                    Dialect = "http://docs.oasis-open.org/wsfed/authorization/200706/authclaims"
                }
            };

            rst.Claims.Add(new RequestClaim("dk:gov:saml:attribute:CvrNumberIdentifier", false, cvr));

            var fullEndpoint = issuer + endpoint;
            var client = GenerateStsCertificateClientChannel(clientCertificate, fullEndpoint);
            return client.Issue(rst);
        }

        private static IWSTrustChannelContract GenerateStsCertificateClientChannel(X509Certificate2 clientCertificate, string endpoint)
        {
            var stsAddress = new EndpointAddress(new Uri(endpoint), EndpointIdentity.CreateDnsIdentity("ADG_EXTTEST_Adgangsstyring_1"));
            var binding = new MutualCertificateWithMessageSecurityBinding(null);
            var factory = new WSTrustChannelFactory(binding, stsAddress);

            factory.TrustVersion = TrustVersion.WSTrust13;
            factory.Credentials.ClientCertificate.Certificate = clientCertificate;
            var certificate = X509CertificateClientCertificateFactory.GetClientCertificate("0aa7a193f18d095f7e2ce09d892178c9682b7924");
            factory.Credentials.ServiceCertificate.ScopedCertificates.Add(stsAddress.Uri, certificate);
            factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
            // Disable revocation checking (do not use in production)
            // Should be uncommented if you intent to call DemoService locally.
            // factory.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            factory.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;


            return factory.CreateChannel();
        }
    }
}
