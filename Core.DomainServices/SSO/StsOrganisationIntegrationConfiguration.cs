namespace Core.DomainServices.SSO
{
    public class StsOrganisationIntegrationConfiguration
    {
        public string CertificateThumbprint { get; }
        public string EndpointHost { get; }

        public StsOrganisationIntegrationConfiguration(string certificateThumbprint, string endpointHost)
        {
            CertificateThumbprint = certificateThumbprint;
            EndpointHost = endpointHost;
        }
    }
}
