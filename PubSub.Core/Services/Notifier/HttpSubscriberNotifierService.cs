using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
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
        public async Task Notify(string message, string recipient)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent($"\"{message}\"", Encoding.UTF8, "application/json");
            var authentication = _callbackAuthenticator.GetAuthentication(message);
            httpClient.DefaultRequestHeaders.Add("Signature-Header", authentication);
            await httpClient.PostAsync(recipient, content);
        }
    }
}
