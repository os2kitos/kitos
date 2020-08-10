using System.Net;

namespace Infrastructure.Services.Http
{
    public static class ServiceEndpointConfiguration
    {
        public static void ConfigureValidationOfOutgoingConnections()
        {
            //Allow all versions of ssl for outgoing connections
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;
        }
    }
}
