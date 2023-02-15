using Core.DomainModel.Commands;
using Core.DomainModel.Users;

namespace Core.ApplicationServices.Model.Authentication.Commands
{
    public class ValidateUserCredentialsCommand : ICommand
    {
        public string Email { get; }
        public string Password { get; }
        public AuthenticationScheme AuthenticationScheme { get; }

        public ValidateUserCredentialsCommand(string email, string password, AuthenticationScheme authenticationScheme)
        {
            Email = email;
            Password = password;
            AuthenticationScheme = authenticationScheme;
        }
    }
}
