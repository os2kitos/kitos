using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Infrastructure.STS.Common.Model.Token;
using Kombit.InfrastructureSamples.AdresseService;
using Kombit.InfrastructureSamples.BrugerService;
using Kombit.InfrastructureSamples.PersonService;
using Kombit.InfrastructureSamples.Token;

namespace Core.DomainServices.SSO
{
    public static class PortFactory
    {
        public static BrugerPortType CreateBrugerPort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvr)
        {
            return ChannelWithIssuedToken(tokenFetcher, configuration, new BrugerPortTypeClient(), cvr);
        }

        public static AdressePortType CreateAdressePort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvr)
        {
            return ChannelWithIssuedToken(tokenFetcher, configuration, new AdressePortTypeClient(), cvr);
        }

        public static PersonPortType CreatePersonPort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvr)
        {
            return ChannelWithIssuedToken(tokenFetcher, configuration, new PersonPortTypeClient(), cvr);
        }

        private static T ChannelWithIssuedToken<T>(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, ClientBase<T> client, string cvr) where T: class
        {
            var token = tokenFetcher.IssueToken(configuration.OrgService6EntityId, cvr);

            var identity = EndpointIdentity.CreateDnsIdentity(configuration.ServiceCertificateAliasOrg);
            var endpointAddress = new EndpointAddress(client.Endpoint.ListenUri, identity);
            client.Endpoint.Address = endpointAddress;
            var certificate = CertificateLoader.LoadCertificate(
                StoreName.My,
                StoreLocation.LocalMachine, configuration.ClientCertificateThumbprint
            );
            client.ClientCredentials.ClientCertificate.Certificate = certificate;
            client.Endpoint.Contract.ProtectionLevel = ProtectionLevel.None;

            return client.ChannelFactory.CreateChannelWithIssuedToken(token);
        }
    }
}