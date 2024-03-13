using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Kombit.InfrastructureSamples.AdresseService;

namespace Core.DomainServices.SSO
{
    internal static class StsAdresseHelpers
    {
        public static laesRequest CreateStsAdresseLaesRequest(string uuid)
        {
            var laesInputType = new LaesInputType {UUIDIdentifikator = uuid};
            var laesRequest = new laesRequest
            {
                LaesInput = laesInputType,
            };
            return laesRequest;
        }

        public static AdressePortTypeClient CreateAdressePortTypeClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            var client = new AdressePortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
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

        public static bool IsStsAdresseObsolete(this RegistreringType8 registreringType8)
        {
            return registreringType8.LivscyklusKode.Equals(LivscyklusKodeType.Slettet) ||
                   registreringType8.LivscyklusKode.Equals(LivscyklusKodeType.Passiveret);
        }
    }
}