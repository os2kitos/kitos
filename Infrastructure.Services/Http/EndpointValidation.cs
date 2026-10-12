namespace Infrastructure.Services.Http
{
    public class EndpointValidation(string url, EndpointValidationError? error = null)
    {
        public string Url { get; } = url;
        public EndpointValidationError? Error { get; } = error;

        public bool Success => Error == null;
    }
}
