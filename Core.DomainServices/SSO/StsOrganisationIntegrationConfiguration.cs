namespace Core.DomainServices.SSO
{
    public class StsOrganisationIntegrationConfiguration
    {
        public string ClientCertificateThumbprint { get; }
        public string EndpointHost { get; }
        public string Issuer { get; }
        public string CertificateEndpoint { get; }
        public string ServiceCertificateAliasOrg { get; }
        public string StsCertificateAlias { get; }
        public string StsCertificateThumbprint { get; }
        public string OrgService6EntityId { get; }

        public StsOrganisationIntegrationConfiguration(string clientCertificateThumbprint, string endpointHost, string issuer, string certificateEndpoint, string serviceCertificateAliasOrg, string stsCertificateAlias, string stsCertificateThumbprint, string orgService6EntityId)
        {
            ClientCertificateThumbprint = clientCertificateThumbprint;
            EndpointHost = endpointHost;
            Issuer = issuer;
            CertificateEndpoint = certificateEndpoint;
            ServiceCertificateAliasOrg = serviceCertificateAliasOrg;
            StsCertificateAlias = stsCertificateAlias;
            StsCertificateThumbprint = stsCertificateThumbprint;
            OrgService6EntityId = orgService6EntityId;
        }
    }
}
