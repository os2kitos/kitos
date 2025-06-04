using Core.DomainModel;
using Infrastructure.Services.Cryptography;

namespace Tools.Test.Database.Model.Extensions
{
    public static class UserExtensions
    {
        public static void SetPassword(this User target, string password)
        {
            using (var cryptoService = new CryptoService())
            {
                target.Password = cryptoService.Encrypt(password + target.Salt);
            }
        }
    }
}
