using System.Web.Security;
using Core.DomainModel;

namespace Core.ApplicationServices.Authentication
{
    public class ApplicationApplicationAuthenticationState : IApplicationAuthenticationState
    {
        public void SetAuthenticatedUser(User user, AuthenticationScope scope)
        {
            FormsAuthentication.SetAuthCookie(user.Id.ToString(), scope == AuthenticationScope.Persistent);
        }
    }
}
