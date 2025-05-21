using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Model.KitosEvents;
using Infrastructure.Services.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Core.Abstractions.Types;
using Serilog;

namespace Core.ApplicationServices.KitosEvents;

public class HttpEventPublisher : IHttpEventPublisher
{
    private readonly IKitosHttpClient _httpClient;
    private readonly IKitosInternalTokenIssuer _tokenIssuer;
    private readonly string _pubSubBaseUrl;
    private readonly ILogger _logger;
    private const string PublishEndpoint = "api/publish";
    public HttpEventPublisher(IKitosHttpClient httpClient, IKitosInternalTokenIssuer tokenIssuer, string pubSubBaseUrl, ILogger logger)
    {
        _httpClient = httpClient;
        _pubSubBaseUrl = pubSubBaseUrl;
        _logger = logger;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<HttpResponseMessage, OperationError>> PostEventAsync(KitosEventDTO eventDTO)
    {
        var token = _tokenIssuer.GetToken();
        if (token.Failed)
        {
            _logger.Fatal("Error in HttpEventPublisher: " + token.Error);
            return token.Error;
        }
        var url = new Uri(new Uri(_pubSubBaseUrl), PublishEndpoint);
        return await _httpClient.PostAsync(eventDTO, url, token.Value.Value);
    }
}