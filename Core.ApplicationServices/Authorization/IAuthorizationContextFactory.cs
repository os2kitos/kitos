namespace Core.ApplicationServices.Authorization
{
    public interface IAuthorizationContextFactory
    {
        IAuthorizationContext Create();
    }
}