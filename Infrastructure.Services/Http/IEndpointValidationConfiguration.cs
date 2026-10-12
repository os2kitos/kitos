using System.Collections.Generic;

namespace Infrastructure.Services.Http;

public interface IEndpointValidationConfiguration
{
    IEnumerable<string> IgnoredProtocols { get; }
}
