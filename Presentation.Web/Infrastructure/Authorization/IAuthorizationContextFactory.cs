namespace Presentation.Web.Infrastructure.Authorization
{
    public interface IAuthorizationContextFactory
    {
        IAuthorizationContext Create();
    }
}