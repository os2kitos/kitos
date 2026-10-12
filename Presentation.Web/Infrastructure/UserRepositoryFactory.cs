using Core.DomainServices;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Presentation.Web.Infrastructure
{
    public class UserRepositoryFactory : IUserRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public UserRepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUserRepository GetUserRepository()
        {
            return _serviceProvider.GetRequiredService<IUserRepository>();
        }
    }
}
