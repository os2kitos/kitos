using System.Text;

namespace PubSub.Core.Services.Notifier
{
    public class HttpSubscriberNotifierService : ISubscriberNotifierService
    {
        private IHttpClientFactory _httpClientFactory;

        public HttpSubscriberNotifierService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task Notify(string message, string recipient)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent($"\"{message}\"", Encoding.UTF8, "application/json");
            await httpClient.PostAsync(recipient, content);
        }
    }
}
