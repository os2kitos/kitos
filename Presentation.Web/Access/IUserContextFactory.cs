namespace Presentation.Web.Access
{
    public interface IUserContextFactory
    {
        IOrganizationalUserContext Create(int userId, int organizationId);
    }
}
