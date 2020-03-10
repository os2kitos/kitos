using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Services.Http
{
    public class EndpointValidationService : IEndpointValidationService
    {
        private static readonly HttpClient Client = new HttpClient();

        public async Task<EndpointValidation> ValidateAsync(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return new EndpointValidation(url, new EndpointValidationError(false));
            }

            using (var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.MovedPermanently:
                        //Will result in pages being shown - redirect may be a "short link" which redirects to the real link
                        return new EndpointValidation(url);
                    default:
                        return new EndpointValidation(url, new EndpointValidationError(true, response.StatusCode));
                }
            }
        }
    }
}
