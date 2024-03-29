﻿using System;
using System.Collections.Generic;
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
        private const string EXCEPTION_TEXT_CouldNotCreateSslTlsSecureChannel = "Could not create SSL/TLS secure channel";

        private static readonly IEnumerable<string> ErrorContentWhichShouldNotBeRetried = new[]
        {
            "Unable to connect to the remote server",
            "An existing connection was forcibly closed by the remote host",
            "The remote name could not be resolved",
            EXCEPTION_TEXT_CouldNotCreateSslTlsSecureChannel
        };

        private readonly ILogger _logger;

        private static readonly HttpClient Client;
        private static readonly IEnumerable<TimeSpan> BackOffDurations = CreateDurations(1, 5, 15).ToList().AsReadOnly();


        static EndpointValidationService()
        {
            //Follow redirects
            Client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = false,
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
            });

            //Set max timeout to 30 in stead of the default 100
            Client.Timeout = TimeSpan.FromSeconds(30);

            //Prevent 406 from servers which require content negotiation
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AnyMediaType));

            //Some servers return 406 if no user agent is set as part of a ModSecure policy
            Client.DefaultRequestHeaders.UserAgent.ParseAdd(ChromeUserAgent);
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
                    switch ((int)response.StatusCode)
                    {
                        case (int)HttpStatusCode.OK:
                        case (int)HttpStatusCode.Redirect:
                        case (int)HttpStatusCode.TemporaryRedirect:
                        case 308: //Permanent redirect: https://tools.ietf.org/html/rfc7238
                        case (int)HttpStatusCode.MovedPermanently:
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
                return new EndpointValidation(url, new EndpointValidationError(MapExceptionError(e)));
            }
        }

        private static EndpointValidationErrorType MapExceptionError(Exception exception)
        {
            var chain = BuildExceptionChain(exception);
            if (chain.IndexOf(EXCEPTION_TEXT_CouldNotCreateSslTlsSecureChannel, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return EndpointValidationErrorType.TlsError;
            }
            return EndpointValidationErrorType.CommunicationError;
        }

        private Task<HttpResponseMessage> LoadEndpointWithBackOffRetryAsync(Uri uri)
        {
            return Policy
                .Handle<Exception>(ShouldRetryException) //outer policy handles transient protocol errors, connection timeouts, task cancellations and so on
                .WaitAndRetryAsync(BackOffDurations, onRetry: HandleFailedRequest)
                .ExecuteAsync(() =>
                {
                    return Policy
                        .HandleResult<HttpResponseMessage>(ShouldRetryFailedResponse) //Inner policy deals with response related errors
                        .WaitAndRetryAsync(BackOffDurations, onRetry: HandleFailedRequest)
                        .ExecuteAsync(() => Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri)));
                });
        }

        private static bool ShouldRetryException(Exception arg)
        {
            if (arg is TaskCanceledException)
            {
                //Timeout due to unresponsive host - do not retry
                return false;
            }
            var exceptionChain = BuildExceptionChain(arg);
            var shouldIgnore = ErrorContentWhichShouldNotBeRetried.Any(content => exceptionChain.IndexOf(content, StringComparison.OrdinalIgnoreCase) != -1);
            return shouldIgnore == false;
        }

        private static IEnumerable<TimeSpan> CreateDurations(params int[] durationInSeconds)
        {
            return durationInSeconds.Select(s => TimeSpan.FromSeconds(s)).ToArray();
        }

        private void HandleFailedRequest(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
        {
            _logger.Warning("EndpointValidation: {correlationId}: Request failed exception chain: '{exnChain}'. Waiting {timeSpan} before next retry. Retry attempt {retryCount}", context.CorrelationId.ToString("D"), BuildExceptionChain(exception), timeSpan, retryCount);
        }

        private static string BuildExceptionChain(Exception exception)
        {
            return exception == null
                ? "END_OF_CHAIN"
                : $"{exception.GetType().Name}('0x{exception.HResult:X}','{exception.Message}')-->{BuildExceptionChain(exception.InnerException)}";
        }

        private void HandleFailedRequest(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount, Context context)
        {
            _logger.Warning("EndpointValidation: {correlationId}: Request failed with {statusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}", context.CorrelationId.ToString("D"), result.Result.StatusCode, timeSpan, retryCount);
            result.Result.Dispose();
        }

        private static bool ShouldRetryFailedResponse(HttpResponseMessage message)
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

                var hostEntry = await Dns.GetHostEntryAsync(uriHost).ConfigureAwait(false);
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
