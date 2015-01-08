using Core.DomainServices;
using Ninject;

namespace Presentation.Web.Infrastructure
{
    public class UserRepositoryFactory : IUserRepositoryFactory
    {
        private readonly IKernel _kernel;

        public UserRepositoryFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IUserRepository GetUserRepository()
        {
            return _kernel.Get<IUserRepository>();
        }
    }
}