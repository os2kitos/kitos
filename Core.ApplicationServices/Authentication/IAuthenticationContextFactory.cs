namespace Core.ApplicationServices.Authentication
{
    public interface IAuthenticationContextFactory
    {
        IAuthenticationContext Create();
    }
}
