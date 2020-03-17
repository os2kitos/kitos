using System.Net;

namespace Infrastructure.Services.Http
{
    public static class ServiceEndpointConfiguration
    {
        public static void ConfigureValidationOfOutgoingConnections()
        {
            //Allow all versions of ssl for outgoing connections
            //NOTE: Once we solve https://os2web.atlassian.net/browse/KITOSUDV-679 we can move the certificate validation to the client handler inside EndpointValidationService.cs in stead of overriding the global configuration
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
        }
    }
}
