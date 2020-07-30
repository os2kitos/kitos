namespace Core.ApplicationServices.Authorization
{
    public interface IUserContextFactory
    {
        IOrganizationalUserContext Create(int userId);
    }
}
