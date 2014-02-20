using Core.DomainServices;
using Ninject;

namespace UI.MVC4.Infrastructure
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