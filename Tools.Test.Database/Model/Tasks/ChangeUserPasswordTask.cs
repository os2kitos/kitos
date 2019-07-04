using System;
using System.Linq;
using Core.ApplicationServices;

namespace Tools.Test.Database.Model.Tasks
{
    public class ChangeUserPasswordTask : DatabaseTask
    {
        private readonly string _email;
        private readonly string _newPassword;

        public ChangeUserPasswordTask(string connectionString, string email, string newPassword)
            : base(connectionString)
        {
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _newPassword = newPassword ?? throw new ArgumentException(nameof(newPassword));
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var user = context.Users.FirstOrDefault(x => x.Email == _email);
                if (user == null)
                {
                    throw new ArgumentException($"No user found with email:{_email}");
                }

                var cryptoService = new CryptoService();
                user.Password = cryptoService.Encrypt(_newPassword + user.Salt);
                context.SaveChanges();
            }

            return true;
        }
    }
}
