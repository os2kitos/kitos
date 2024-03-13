using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.Abstractions.Types;
using Infrastructure.STS.Common.Model.Token;
using Kombit.InfrastructureSamples.AdresseService;
using Kombit.InfrastructureSamples.BrugerService;
using Kombit.InfrastructureSamples.PersonService;
using Kombit.InfrastructureSamples.Token;

namespace Core.DomainServices.SSO
{
    public static class PortFactory
    {
        private const string ERROR_RECEIVED_USER_CONTEXT_DOES_NOT_EXIST_ON_USING_SYSTEM = "5015";

        public static Result<BrugerPortType, string> CreateBrugerPort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvrNumber)
        {
            return ChannelWithIssuedToken(tokenFetcher, configuration, new BrugerPortTypeClient(), cvrNumber);
        }

        public static Result<AdressePortType,string> CreateAdressePort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvrNumber)
        {
            return ChannelWithIssuedToken(tokenFetcher, configuration, new AdressePortTypeClient(), cvrNumber);
        }

        public static Result<PersonPortType,string> CreatePersonPort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvrNumber)
        {
            return ChannelWithIssuedToken(tokenFetcher, configuration, new PersonPortTypeClient(), cvrNumber);
        }

        private static Result<T, string> ChannelWithIssuedToken<T>(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, ClientBase<T> client, string cvrNumber) where T: class
        {
            try
            {
                var token = tokenFetcher.IssueToken(configuration.OrgService6EntityId, cvrNumber);
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
            catch (FaultException e)
            {
                if (e.Code.Name == ERROR_RECEIVED_USER_CONTEXT_DOES_NOT_EXIST_ON_USING_SYSTEM)
                {
                    return $"No service agreement on municipality with CVR {cvrNumber}";
                }
                return $"SSO unknown general error: {e.Message}";
            }
        }
    }
}