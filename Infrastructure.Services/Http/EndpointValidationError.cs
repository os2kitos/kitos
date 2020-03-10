using System.Net;

namespace Infrastructure.Services.Http
{
    public class EndpointValidationError
    {
        public bool ValidUri { get; }
        public HttpStatusCode? StatusCode { get; }

        public EndpointValidationError(bool validUri, HttpStatusCode? statusCode = null)
        {
            ValidUri = validUri;
            StatusCode = statusCode;
        }
    }
}
