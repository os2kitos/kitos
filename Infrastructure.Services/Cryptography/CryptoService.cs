using System;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.Cryptography
{
    public class CryptoService : ICryptoService
    {
        public string Encrypt(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] encrypted = SHA256.HashData(bytes);
            return UrlTokenEncode(encrypted);
        }

        public void Dispose()
        {
            // SHA256.HashData is static — nothing to dispose
        }

        /// <summary>
        /// Reimplements System.Web.HttpServerUtility.UrlTokenEncode so stored hashes
        /// remain identical after the System.Web removal.
        /// </summary>
        private static string UrlTokenEncode(byte[] input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;

            string base64 = Convert.ToBase64String(input);
            int endPos = base64.Length;
            int paddingCount = 0;
            while (endPos > 0 && base64[endPos - 1] == '=')
            {
                endPos--;
                paddingCount++;
            }

            char[] chars = new char[endPos + 1];
            chars[endPos] = (char)('0' + paddingCount);
            for (int i = 0; i < endPos; i++)
            {
                chars[i] = base64[i] switch
                {
                    '+' => '-',
                    '/' => '_',
                    var c => c
                };
            }
            return new string(chars);
        }
    }
}
