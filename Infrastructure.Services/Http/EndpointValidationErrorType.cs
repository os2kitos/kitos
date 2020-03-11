namespace Infrastructure.Services.Http
{
    public enum EndpointValidationErrorType
    {
        InvalidUriFormat,
        InvalidWebsiteUri,
        DnsLookupFailed,
        CommunicationError,
        ErrorResponseCode
    }
}
