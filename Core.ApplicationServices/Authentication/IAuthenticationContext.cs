namespace Core.ApplicationServices.Authentication
{
    public interface IAuthenticationContext
    {
        AuthenticationMethod Method { get; }

        int? UserId { get; }

        bool HasApiAccess { get; }
    }
}
