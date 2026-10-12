using System.Collections.Generic;

namespace Infrastructure.Services.Http
{
    public class EndpointValidationConfiguration : IEndpointValidationConfiguration
    {
        public IEnumerable<string> IgnoredProtocols => ["kmdsageraabn", "kmdedhvis", "sbsyslauncher"];
    }
}
