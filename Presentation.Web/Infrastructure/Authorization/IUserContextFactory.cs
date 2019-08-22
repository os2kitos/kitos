namespace Presentation.Web.Infrastructure.Authorization
{
    public interface IUserContextFactory
    {
        IOrganizationalUserContext Create(int? userId, int organizationId);
    }
}
