namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public interface IUserContextFactory
    {
        IOrganizationalUserContext Create(int userId, int organizationId);
    }
}
