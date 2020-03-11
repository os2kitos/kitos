﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Infrastructure.Services.Extensions;
using Polly;
using Serilog;

namespace Infrastructure.Services.Http
{
    public class EndpointValidationService : IEndpointValidationService
    {
        private const string ChromeUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36";
        private const string AnyMediaType = "*/*";

        private readonly ILogger _logger;

        private static readonly HttpClient Client;

        static EndpointValidationService()
        {
            //Follow redirects
            Client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = false
            });

            //Set max timeout to 30 in stead of the default 100
            Client.Timeout = TimeSpan.FromSeconds(30);

            //Prevent 406 from servers which require content negotiation
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AnyMediaType));

            //Some servers return 406 if no user agent is set as part of a ModSecure policy
            Client.DefaultRequestHeaders.UserAgent.ParseAdd(ChromeUserAgent);

            Client.DefaultRequestHeaders.ExpectContinue = false;
        }

        public EndpointValidationService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<EndpointValidation> ValidateAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new EndpointValidation(url, new EndpointValidationError(EndpointValidationErrorType.InvalidUriFormat));
            }
            try
            {
                _logger.Debug("Checking Endpoint at: '{uri}'", url);
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

                using (var response = await LoadEndpointWithBackOffRetryAsync(uri).ConfigureAwait(false))
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MovedPermanently:
                            //Will result in pages being shown - redirect might be a "short link" which redirects to the real link
                            return new EndpointValidation(url);
                        default:
                            return new EndpointValidation(url, new EndpointValidationError(EndpointValidationErrorType.ErrorResponseCode, response.StatusCode));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Information(e, "Failed to validate url {url}", url);

                //This is typically where we end up if we get a connection timeout or other type of communication error where the http client is unable to proceed
                return new EndpointValidation(url, new EndpointValidationError(EndpointValidationErrorType.CommunicationError));
            }
        }

        private Task<HttpResponseMessage> LoadEndpointWithBackOffRetryAsync(Uri uri)
        {
            return Policy
                .HandleResult<HttpResponseMessage>(ShouldRetry)
                .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15) }, onRetry: HandleFailedRequest)
                .ExecuteAsync(() => Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri)));
        }

        private void HandleFailedRequest(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount, Context context)
        {
            _logger.Warning("{correlationId}: Request failed with {statusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}", context.CorrelationId.ToString("D"), result.Result.StatusCode, timeSpan, retryCount);
            result.Result.Dispose();
        }

        private static bool ShouldRetry(HttpResponseMessage message)
        {
            return MatchAnyStatusCode(message,
                HttpStatusCode.ServiceUnavailable,  //Either it is down or we are being throttled
                HttpStatusCode.InternalServerError,             //Might be transient server error
                HttpStatusCode.GatewayTimeout,                  //Timeout at the proxy level
                HttpStatusCode.BadGateway);                     //Error at the proxy level
        }

        private static bool MatchAnyStatusCode(HttpResponseMessage message, params HttpStatusCode[] statuses)
        {
            return statuses.Contains(message.StatusCode);
        }

        private static async Task<bool> CanResolveHostAsync(Uri uri)
        {
            try
            {
                var uriHost = uri.Host;

                if (IsIpAddress(uriHost))
                {
                    //Does not require DNS lookup
                    return true;
                }

                var hostEntry = await Dns.GetHostEntryAsync(uriHost);
                return hostEntry?.AddressList?.Any() == true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsIpAddress(string uriHost)
        {
            return IPAddress.TryParse(uriHost, out var ip);
        }
    }
}
