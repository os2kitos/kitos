using Core.DomainModel;

namespace Core.ApplicationServices.Authentication
{
    public interface IApplicationAuthenticationState
    {
        void SetAuthenticatedUser(User user, AuthenticationScope scope);
    }
}
