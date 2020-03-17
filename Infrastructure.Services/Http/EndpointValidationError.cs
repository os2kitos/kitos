using System.Net;

namespace Infrastructure.Services.Http
{
    public class EndpointValidationError
    {
        public EndpointValidationErrorType ErrorType { get; }
        public HttpStatusCode? StatusCode { get; }

        public EndpointValidationError(EndpointValidationErrorType errorType, HttpStatusCode? statusCode = null)
        {
            ErrorType = errorType;
            StatusCode = statusCode;
        }
    }
}
