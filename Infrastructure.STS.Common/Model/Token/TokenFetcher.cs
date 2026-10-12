using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Text;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.OioWsTrustCore;
using Infrastructure.STS.Common.Model.Token;

namespace Kombit.InfrastructureSamples.Token;

public class TokenFetcher
{
    private readonly TokenFetcherConfig _config;

    public TokenFetcher(TokenFetcherConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Checks if a token is valid. 5 minutes added to be sure it is valid.
    /// </summary>
    public bool IsTokenExpired(SecurityToken token)
    {
        return token.ValidTo > DateTime.Now.AddMinutes(5);
    }

    /// <summary>
    /// Creates a token for use on Serviceplatformen using Kombit.OioIdws.WscCore.
    /// </summary>
    public SecurityToken IssueToken(string entityId, string cvr)
    {
        var absoluteUri = new Uri(entityId).AbsoluteUri;
        var cacheKey = new Guid(MD5.HashData(Encoding.Default.GetBytes(absoluteUri + "_" + cvr))).ToString();
        var inCache = CacheHelper.IsIncache(cacheKey);

        if (inCache)
        {
            var cached = CacheHelper.GetFromCache<GenericXmlSecurityToken>(cacheKey);
            if (cached != null && cached.ValidTo.CompareTo(DateTime.Now) >= 0)
                return cached;
        }

        var token = FetchToken(absoluteUri, cvr);
        CacheHelper.SaveTocache(cacheKey, token, token.ValidTo);
        return token;
    }

    /// <summary>
    /// Builds a StsTokenServiceConfiguration with the correct WSP service endpoint URL.
    /// Use for creating WCF channels via FederatedChannelFactoryExtensions.CreateChannelWithIssuedToken.
    /// </summary>
    public StsTokenServiceConfiguration BuildServiceConfig(string entityId, string wspEndpoint, string cvr)
    {
        var clientCertificate = CertificateLoader.LoadCertificateWithFallback(
            _config.SsoCertFilePath, _config.SsoCertPassword,
            StoreName.My, StoreLocation.LocalMachine, _config.ClientCertificateThumbprint);
        var stsCertificate = CertificateLoader.LoadCertificateWithFallback(
            _config.StsCertFilePath, _config.StsCertPassword,
            StoreName.My, StoreLocation.LocalMachine, _config.StsCertificateThumbprint);
        var wspCertificate = CertificateLoader.LoadCertificateWithFallback(
            _config.StsOrganisationCertFilePath, _config.StsOrganisationCertPassword,
            StoreName.My, StoreLocation.LocalMachine, _config.WspCertificateThumbprint);
        var stsConfiguration = new StsConfiguration(_config.StsEndpoint, _config.StsIssuer, cvr, stsCertificate);
        var wspConfiguration = new WspConfiguration(wspEndpoint, entityId, System.ServiceModel.EnvelopeVersion.Soap12, wspCertificate);
        var tokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, clientCertificate)
        {
            MaxReceivedMessageSize = int.MaxValue
        };
        ConfigureServiceCertificateAuthentication(tokenServiceConfiguration);
        return tokenServiceConfiguration;
    }

    /// <summary>
    /// Issues a STS token and creates a WCF channel to <paramref name="wspEndpoint"/>.
    /// Uses <see cref="FixedCreateChannelExtensions"/> instead of the Kombit
    /// <see cref="Digst.OioIdws.SoapCore.FederatedChannelFactoryExtensions"/> to work around
    /// a .NET 10 WCF incompatibility in <c>FederatedChannelFactory.ApplyConfiguration</c>.
    /// </summary>
    public T CreateChannel<T>(string entityId, string wspEndpoint, string cvr)
    {
        var token = (GenericXmlSecurityToken)IssueToken(entityId, cvr);
        var config = BuildServiceConfig(entityId, wspEndpoint, cvr);
        return FixedCreateChannelExtensions.CreateChannelWithIssuedToken<T>(token, config);
    }

    private SecurityToken FetchToken(string entityId, string cvr)
    {
        var config = BuildServiceConfig(entityId, entityId, cvr);
        return new StsTokenService(config).GetToken();
    }

    private void ConfigureServiceCertificateAuthentication(IStsTokenServiceConfiguration config)
    {
        ApplyAuthenticationOverrides(config.StsCertificateAuthentication);
        ApplyAuthenticationOverrides(config.WspCertificateAuthentication);
        ApplyAuthenticationOverrides(config.SslCertificateAuthentication);
    }

    private void ApplyAuthenticationOverrides(X509ServiceCertificateAuthentication authentication)
    {
        if (_config.ParsedCertificateValidationMode.HasValue)
            authentication.CertificateValidationMode = _config.ParsedCertificateValidationMode.Value;
        if (_config.ParsedRevocationMode.HasValue)
            authentication.RevocationMode = _config.ParsedRevocationMode.Value;
    }

}
