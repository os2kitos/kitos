using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Model.KitosEvents;
using Infrastructure.Services.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Core.Abstractions.Types;

namespace Core.ApplicationServices.KitosEvents;

public class HttpEventPublisher : IHttpEventPublisher
{
    private readonly IKitosHttpClient _httpClient;
    private readonly IKitosInternalTokenIssuer _tokenIssuer;
    private readonly string _pubSubBaseUrl;
    private const string PublishEndpoint = "api/publish";
    public HttpEventPublisher(IKitosHttpClient httpClient, IKitosInternalTokenIssuer tokenIssuer, string pubSubBaseUrl)
    {
        _httpClient = httpClient;
        _pubSubBaseUrl = pubSubBaseUrl;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<HttpResponseMessage, OperationError>> PostEventAsync(KitosEventDTO eventDTO)
    {
        var token = _tokenIssuer.GetToken();
        if (token.Failed)
        {
            return token.Error;
        }
        var url = new Uri(new Uri(_pubSubBaseUrl), PublishEndpoint);
        return await _httpClient.PostAsync(eventDTO, url, token.Value.Value);
    }
}