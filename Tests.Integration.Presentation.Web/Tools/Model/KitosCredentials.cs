using Core.DomainModel.Organization;

namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class KitosCredentials
    {
        public string Username { get; }
        public string Password { get; }
        public OrganizationRole Role { get; }

        public KitosCredentials(string username, string password, OrganizationRole role)
        {
            Username = username;
            Password = password;
            Role = role;
        }
    }
}
