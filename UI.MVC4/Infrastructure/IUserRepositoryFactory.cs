using Core.DomainServices;

namespace UI.MVC4.Infrastructure
{
    public interface IUserRepositoryFactory
    {
        IUserRepository GetUserRepository();
    }
}