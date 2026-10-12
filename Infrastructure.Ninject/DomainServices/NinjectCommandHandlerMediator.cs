using System;
using Core.DomainModel.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Ninject.DomainServices
{
    public class NinjectCommandHandlerMediator : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider;

        public NinjectCommandHandlerMediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TResult Execute<TCommand, TResult>(TCommand args) where TCommand : ICommand
        {
            return _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>().Execute(args);
        }
    }
}
