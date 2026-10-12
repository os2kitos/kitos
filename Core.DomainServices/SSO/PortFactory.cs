using System.ServiceModel;
using Core.Abstractions.Types;
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
            return ChannelWithIssuedToken<BrugerPortType>(tokenFetcher, configuration,
                configuration.BrugerServiceUrl, cvrNumber);
        }

        public static Result<AdressePortType, string> CreateAdressePort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvrNumber)
        {
            return ChannelWithIssuedToken<AdressePortType>(tokenFetcher, configuration,
                configuration.AdresseServiceUrl, cvrNumber);
        }

        public static Result<PersonPortType, string> CreatePersonPort(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string cvrNumber)
        {
            return ChannelWithIssuedToken<PersonPortType>(tokenFetcher, configuration,
                configuration.PersonServiceUrl, cvrNumber);
        }

        private static Result<T, string> ChannelWithIssuedToken<T>(TokenFetcher tokenFetcher, StsOrganisationIntegrationConfiguration configuration, string serviceEndpointUrl, string cvrNumber) where T : class
        {
            try
            {
                return tokenFetcher.CreateChannel<T>(configuration.OrgService6EntityId, serviceEndpointUrl, cvrNumber);
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