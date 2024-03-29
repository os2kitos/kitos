﻿using Core.DomainModel.Commands;
using Ninject;

namespace Infrastructure.Ninject.DomainServices
{
    public class NinjectCommandHandlerMediator : ICommandBus
    {
        public TResult Execute<TCommand, TResult>(TCommand args) where TCommand : ICommand
        {
            return _kernel.Get<ICommandHandler<TCommand, TResult>>().Execute(args);
        }

        private readonly IKernel _kernel;

        public NinjectCommandHandlerMediator(IKernel kernel)
        {
            _kernel = kernel;
        }
    }
}
