using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Infrastructure.Factories.Authentication
{
    public interface IAuthenticationContextFactory
    {
        IAuthenticationContext Create();
    }
}
