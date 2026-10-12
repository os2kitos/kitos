using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace Kombit.InfrastructureSamples.Token;

public class TokenFetcherConfig
{
    public string ClientCertificateThumbprint { get; }
    public string StsIssuer { get; }
    public string StsEndpoint { get; }
    public string StsCertificateAlias { get; }
    public string StsCertificateThumbprint { get; }
    public string WspCertificateThumbprint { get; }

    public string? SsoCertFilePath { get; }
    public string? SsoCertPassword { get; }
    public string? StsCertFilePath { get; }
    public string? StsCertPassword { get; }
    public string? StsOrganisationCertFilePath { get; }
    public string? StsOrganisationCertPassword { get; }
    public string? ServiceCertificateValidationMode { get; }
    public string? ServiceCertificateRevocationMode { get; }

    public TokenFetcherConfig(
        string clientCertificateThumbprint,
        string stsIssuer,
        string stsEndpoint,
        string stsCertificateAlias,
        string stsCertificateThumbprint,
        string wspCertificateThumbprint,
        string? ssoCertFilePath = null,
        string? ssoCertPassword = null,
        string? stsCertFilePath = null,
        string? stsCertPassword = null,
        string? stsOrganisationCertFilePath = null,
        string? stsOrganisationCertPassword = null,
        string? serviceCertificateValidationMode = null,
        string? serviceCertificateRevocationMode = null)
    {
        ClientCertificateThumbprint = clientCertificateThumbprint;
        StsIssuer = stsIssuer;
        StsEndpoint = stsEndpoint;
        StsCertificateAlias = stsCertificateAlias;
        StsCertificateThumbprint = stsCertificateThumbprint;
        WspCertificateThumbprint = wspCertificateThumbprint;
        SsoCertFilePath = ssoCertFilePath;
        SsoCertPassword = ssoCertPassword;
        StsCertFilePath = stsCertFilePath;
        StsCertPassword = stsCertPassword;
        StsOrganisationCertFilePath = stsOrganisationCertFilePath;
        StsOrganisationCertPassword = stsOrganisationCertPassword;
        ServiceCertificateValidationMode = serviceCertificateValidationMode;
        ServiceCertificateRevocationMode = serviceCertificateRevocationMode;
    }

    internal X509CertificateValidationMode? ParsedCertificateValidationMode =>
        EnumParsingHelper.ParseOptionalEnum<X509CertificateValidationMode>(
            ServiceCertificateValidationMode, 
            nameof(ServiceCertificateValidationMode));

    internal X509RevocationMode? ParsedRevocationMode => 
        EnumParsingHelper.ParseOptionalEnum<X509RevocationMode>(
            ServiceCertificateRevocationMode, 
            nameof(ServiceCertificateRevocationMode));
}
