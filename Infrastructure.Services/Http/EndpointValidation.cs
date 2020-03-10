using System;

namespace Infrastructure.Services.Http
{
    public class EndpointValidation
    {
        public string Url { get; }
        public EndpointValidationError Error { get; }

        public EndpointValidation(string url, EndpointValidationError error = null)
        {
            Url = url;
            Error = error;
        }

        public bool Success => Error == null;
    }
}
