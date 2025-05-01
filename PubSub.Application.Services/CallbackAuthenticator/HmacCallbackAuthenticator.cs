using System.Security.Cryptography;
using System.Text;
using PubSub.Core.ApplicationServices.Config;

namespace PubSub.Core.ApplicationServices.CallbackAuthenticator
{
    public class HmacCallbackAuthenticator : ICallbackAuthenticator
    {
        private readonly ICallbackAuthenticatorConfig _configuration;

        public HmacCallbackAuthenticator(ICallbackAuthenticatorConfig configuration)
        {
            _configuration = configuration;
        }

        public string GetAuthentication(string source)
        {
            var apiKey = _configuration.ApiKey;
            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(source));
            return Convert.ToBase64String(hash);
        }
    }
}
