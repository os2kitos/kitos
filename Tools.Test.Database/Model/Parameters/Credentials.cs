using System;

namespace Tools.Test.Database.Model.Parameters
{
    public class Credentials
    {
        public string Email { get; }
        public string Password { get; }

        public Credentials(string email, string password)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
    }
}
