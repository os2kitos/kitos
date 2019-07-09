using Core.ApplicationServices;
using Core.DomainModel;

namespace Tools.Test.Database.Model.Extensions
{
    public static class UserExtensions
    {
        public static void SetPassword(this User target, string password)
        {
            var cryptoService = new CryptoService();
            target.Password = cryptoService.Encrypt(password + target.Salt);
        }
    }
}
