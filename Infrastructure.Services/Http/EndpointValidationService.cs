using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Infrastructure.Services.Extensions;

namespace Infrastructure.Services.Http
{
    public class EndpointValidationService : IEndpointValidationService
    {
        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });

        public async Task<EndpointValidation> ValidateAsync(string url)
        {
            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return new EndpointValidation(url, new EndpointValidationError(EndpointValidationErrorType.InvalidUriFormat));
                }

                if (!uri.IsHttpUri())
                {
                    return new EndpointValidation(url, new EndpointValidationError(EndpointValidationErrorType.InvalidWebsiteUri));
                }

                if (!await CanResolveHostAsync(uri).ConfigureAwait(false))
                {
                    return new EndpointValidation(url, new EndpointValidationError(EndpointValidationErrorType.DnsLookupFailed));
                }

                using (var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri)))
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MovedPermanently:
                            //Will result in pages being shown - redirect may be a "short link" which redirects to the real link
                            return new EndpointValidation(url);
                        default:
                            return new EndpointValidation(url, new EndpointValidationError(EndpointValidationErrorType.ErrorResponse, response.StatusCode));
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Log and fail
                Debug.WriteLine($"FAILED:{e.Message}");
                return new EndpointValidation(url);
            }
        }

        private static async Task<bool> CanResolveHostAsync(Uri uri)
        {
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(uri.Host);
                return hostEntry?.AddressList?.Any() == true;
            }
            catch
            {
                return false;
            }
        }
    }
}
