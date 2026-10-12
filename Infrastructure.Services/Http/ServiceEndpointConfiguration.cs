namespace Infrastructure.Services.Http
{
    public static class ServiceEndpointConfiguration
    {
        public static void ConfigureValidationOfOutgoingConnections()
        {
            // No-op on .NET Core: ServicePointManager is not supported.
            // SSL/TLS protocol configuration is handled per-client via HttpClientHandler
            // (see EndpointValidationService static constructor).
        }
    }
}
