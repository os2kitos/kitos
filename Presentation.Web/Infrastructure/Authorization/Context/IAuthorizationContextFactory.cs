namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public interface IAuthorizationContextFactory
    {
        IAuthorizationContext Create();
    }
}