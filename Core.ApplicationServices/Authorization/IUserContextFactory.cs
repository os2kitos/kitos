namespace Core.ApplicationServices.Authorization
{
    public interface IUserContextFactory
    {
        //TODO: Remove organization id
        IOrganizationalUserContext Create(int userId, int organizationId);
    }
}
