namespace Core.DomainServices.SSO
{
    public class StsOrganisationIntegrationConfiguration
    {
        public string ClientCertificateThumbprint { get; }
        public string Issuer { get; }
        public string CertificateEndpoint { get; }
        public string ServiceCertificateAliasOrg { get; }
        public string StsCertificateAlias { get; }
        public string StsCertificateThumbprint { get; }
        public string OrgService6EntityId { get; }

        public string VirksomhedServiceUrl { get; }
        public string OrganisationServiceUrl { get; }
        public string OrganisationSystemServiceUrl { get; }
        public string AdresseServiceUrl { get; }
        public string BrugerServiceUrl { get; }
        public string PersonServiceUrl { get; }

        public StsOrganisationIntegrationConfiguration(
            string clientCertificateThumbprint,
            string issuer,
            string certificateEndpoint,
            string serviceCertificateAliasOrg,
            string stsCertificateAlias,
            string stsCertificateThumbprint,
            string orgService6EntityId,
            string virksomhedServiceUrl,
            string organisationServiceUrl,
            string organisationSystemServiceUrl,
            string adresseServiceUrl,
            string brugerServiceUrl,
            string personServiceUrl)
        {
            ClientCertificateThumbprint = clientCertificateThumbprint;
            Issuer = issuer;
            CertificateEndpoint = certificateEndpoint;
            ServiceCertificateAliasOrg = serviceCertificateAliasOrg;
            StsCertificateAlias = stsCertificateAlias;
            StsCertificateThumbprint = stsCertificateThumbprint;
            OrgService6EntityId = orgService6EntityId;
            VirksomhedServiceUrl = virksomhedServiceUrl;
            OrganisationServiceUrl = organisationServiceUrl;
            OrganisationSystemServiceUrl = organisationSystemServiceUrl;
            AdresseServiceUrl = adresseServiceUrl;
            BrugerServiceUrl = brugerServiceUrl;
            PersonServiceUrl = personServiceUrl;
        }
    }
}
