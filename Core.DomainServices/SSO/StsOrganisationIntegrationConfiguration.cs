namespace Core.DomainServices.SSO
{
    public class StsOrganisationIntegrationConfiguration
    {
        public string CertificateThumbprint { get; }
        public string EndpointHost { get; }
        public string Issuer { get; }
        public string CertificateEndpoint { get; }

        public StsOrganisationIntegrationConfiguration(string certificateThumbprint, string endpointHost, string issuer, string certificateEndpoint)
        {
            CertificateThumbprint = certificateThumbprint;
            EndpointHost = endpointHost;
            Issuer = issuer;
            CertificateEndpoint = certificateEndpoint;
        }
    }
}
