using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Kombit.InfrastructureSamples.PersonService;

namespace Core.DomainServices.SSO
{
    internal static class StsPersonHelpers
    {
        public static laesRequest CreateStsPersonLaesRequest(string uuid)
        {
            var laesInputType = new LaesInputType {UUIDIdentifikator = uuid};
            var laesRequest = new laesRequest
            {
                LaesInput = laesInputType
            };
            return laesRequest;
        }

        public static PersonPortTypeClient CreatePersonPortTypeClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            var client = new PersonPortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
            {
                ClientCredentials =
                {
                    ClientCertificate =
                    {
                        Certificate = certificate
                    }
                }
            };
            return client;
        }

        public static bool IsStsPersonObsolete(this RegistreringType10 registreringType10)
        {
            return registreringType10.LivscyklusKode.Equals(LivscyklusKodeType.Slettet) ||
                   registreringType10.LivscyklusKode.Equals(LivscyklusKodeType.Passiveret);
        }
    }}
