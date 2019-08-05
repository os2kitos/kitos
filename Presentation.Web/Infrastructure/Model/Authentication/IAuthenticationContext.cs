namespace Presentation.Web.Infrastructure.Model.Authentication
{
    public interface IAuthenticationContext
    {
        AuthenticationMethod Method { get; }
        int? UserId { get; }
        int? ActiveOrganizationId { get; }
    }
}
