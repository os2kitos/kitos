namespace Core.DomainServices.SSO
{
    public class StsOrganisationIntegrationConfiguration
    {
        public string CertificateThumbprint { get; }
        public string EndpointHost { get; }
        public string AuthorizedMunicipalityCvr { get; }

        public StsOrganisationIntegrationConfiguration(string certificateThumbprint, string endpointHost, string authorizedMunicipalityCvr)
        {
            CertificateThumbprint = certificateThumbprint;
            EndpointHost = endpointHost;
            AuthorizedMunicipalityCvr = authorizedMunicipalityCvr;
        }
    }
}
