using System.Text;
using System.Text.Json;
using PubSub.Core.Models;
using PubSub.Core.Services.CallbackAuthentication;

namespace PubSub.Core.Services.Notifier
{
    public class HttpSubscriberNotifierService : ISubscriberNotifierService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICallbackAuthenticator _callbackAuthenticator;

        public HttpSubscriberNotifierService(IHttpClientFactory httpClientFactory, ICallbackAuthenticator callbackAuthenticator)
        {
            _httpClientFactory = httpClientFactory;
            _callbackAuthenticator = callbackAuthenticator;
        }
        public async Task Notify(JsonElement payload, string recipient)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var publicationDto = new PublicationDTO(payload);

            string jsonPayload = JsonSerializer.Serialize(publicationDto);

            var authentication = _callbackAuthenticator.GetAuthentication(jsonPayload);
            httpClient.DefaultRequestHeaders.Add("Signature-Header", authentication);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            await httpClient.PostAsync(recipient, content);
        }


    }
}
