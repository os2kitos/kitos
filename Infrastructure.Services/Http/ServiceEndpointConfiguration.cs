using System;
using System.Linq;
using System.Net;

namespace Infrastructure.Services.Http
{
    public static class ServiceEndpointConfiguration
    {
        public static void ConfigureValidationOfOutgoingConnections()
        {
            //Allow all versions of ssl for outgoing connections
            ServicePointManager.SecurityProtocol = Enum
                .GetValues(typeof(SecurityProtocolType))
                .Cast<SecurityProtocolType>()
                .Aggregate(SecurityProtocolType.SystemDefault, (acc, next) => acc | next);
        }
    }
}
