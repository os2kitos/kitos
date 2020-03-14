using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Infrastructure.Soap.STSAdresse;

namespace Core.ApplicationServices.SSO
{
    internal static class StsAdresseHelpers
    {
        public static laesRequest CreateStsAdresseLaesRequest(string municipalityCvr, string uuid)
        {
            var laesInputType = new LaesInputType {UUIDIdentifikator = uuid};
            var laesRequest = new laesRequest
            {
                LaesRequest1 = new LaesRequestType
                {
                    LaesInput = laesInputType,
                    AuthorityContext = new AuthorityContextType()
                    {
                        MunicipalityCVR = municipalityCvr 
                    }
                }
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
    }
}