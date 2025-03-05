using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PubSub.Core.Services.Notifier
{
    public class HttpSubscriberNotifierService : ISubscriberNotifierService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HttpSubscriberNotifierService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task Notify(string message, string recipient)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent($"\"{message}\"", Encoding.UTF8, "application/json");
            var hmac = GetHmac(message);
            httpClient.DefaultRequestHeaders.Add("Signature-Header", hmac);
            await httpClient.PostAsync(recipient, content);
        }

        private string GetHmac(string text)
        {
            var key = _configuration["ApiKey"] ?? "";

            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToBase64String(hash);
        }
    }
}
