namespace Core.ApplicationServices.Authentication
{
    public class AuthenticationContext : IAuthenticationContext
    {
        public AuthenticationMethod Method { get; }
        public int? UserId { get; }
        public bool HasApiAccess { get; }

        public AuthenticationContext(
            AuthenticationMethod method,
            bool hasApiAccess,
            int? userId = null)
        {
            Method = method;
            UserId = userId;
            HasApiAccess = hasApiAccess;
        }
    }
}