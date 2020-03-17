namespace Infrastructure.Services.Http
{
    public enum EndpointValidationErrorType
    {
        InvalidUriFormat = 0,
        InvalidWebsiteUri = 1,
        DnsLookupFailed = 2,
        CommunicationError = 3,
        ErrorResponseCode = 4
    }
}
