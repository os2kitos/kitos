using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Infrastructure.Services.Cryptography
{
    public class CryptoService : ICryptoService
    {
        private readonly SHA256Managed _crypt;

        public CryptoService()
        {
            _crypt = new SHA256Managed();
        }

        public string Encrypt(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] encrypted = _crypt.ComputeHash(bytes);

            return HttpServerUtility.UrlTokenEncode(encrypted);
        }

        public void Dispose()
        {
            _crypt?.Dispose();
        }
    }
}
