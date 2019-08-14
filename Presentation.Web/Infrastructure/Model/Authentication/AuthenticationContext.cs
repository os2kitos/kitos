namespace Presentation.Web.Infrastructure.Model.Authentication
{
    public class AuthenticationContext : IAuthenticationContext
    {
        public AuthenticationMethod Method { get; }
        public int? UserId { get; }
        public int? ActiveOrganizationId { get; }
        public bool HasApiAccess { get; }

        public AuthenticationContext(
            AuthenticationMethod method,
            bool hasApiAccess,
            int? userId = null, 
            int? activeOrganizationId = null)
        {
            Method = method;
            UserId = userId;
            ActiveOrganizationId = activeOrganizationId;
            HasApiAccess = hasApiAccess;
        }
    }
}