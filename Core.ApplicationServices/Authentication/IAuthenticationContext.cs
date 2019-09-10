namespace Core.ApplicationServices.Authentication
{
    public interface IAuthenticationContext
    {
        AuthenticationMethod Method { get; }
        int? UserId { get; }
        int? ActiveOrganizationId { get; }

        bool HasApiAccess { get; }
    }
}
