using Core.DomainServices;

namespace Presentation.Web.Infrastructure
{
    public interface IUserRepositoryFactory
    {
        IUserRepository GetUserRepository();
    }
}