using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Infrastructure.Soap.STSBruger;

namespace Core.ApplicationServices.SSO
{
    internal static class StsBrugerHelpers
    {
        public static laesRequest CreateStsBrugerLaesRequest(string municipalityCvr, string uuid)
        {
            var laesInputType = new LaesInputType {UUIDIdentifikator = uuid};
            var laesRequest = new laesRequest
            {
                LaesRequest1 = new LaesRequestType
                {
                    LaesInput = laesInputType,
                    AuthorityContext = new AuthorityContextType
                    {
                        MunicipalityCVR = municipalityCvr 
                    }
                }
            };
            return laesRequest;
        }

        public static BrugerPortTypeClient CreateBrugerPortTypeClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            var client = new BrugerPortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
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