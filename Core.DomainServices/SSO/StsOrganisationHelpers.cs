using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Infrastructure.Soap.STSOrganisation;
using AuthorityContextType = Infrastructure.Soap.STSOrganisation.AuthorityContextType;
using LaesInputType = Infrastructure.Soap.STSOrganisation.LaesInputType;
using laesRequest = Infrastructure.Soap.STSOrganisation.laesRequest;
using LaesRequestType = Infrastructure.Soap.STSOrganisation.LaesRequestType;

namespace Core.DomainServices.SSO
{
    internal static class StsOrganisationHelpers
    {
        public static laesRequest CreateStsOrganisationLaesRequest(string municipalityCvr, string uuid)
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

        public static OrganisationPortTypeClient CreateOrganisationPortTypeClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            var client = new OrganisationPortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
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

        public static bool IsStsOrganisationObsolete(this RegistreringType1 registreringType1)
        {
            return registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Passiveret) ||
                   registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Slettet);
        }
    }
}
