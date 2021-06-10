namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class KitosCredentials
    {
        public string Username { get; }
        public string Password { get; }

        public KitosCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
