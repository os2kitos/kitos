using Core.Abstractions.Types;
using Core.DomainModel.Commands;

namespace Core.DomainModel.Extensions
{
    public static class CommandBusExtensions
    {
        public static Result<TResult, OperationError> ExecuteWithResult<TCommand, TResult>(this ICommandBus bus, TCommand command) where TCommand : ICommand
        {
            return bus.Execute<TCommand, Result<TResult, OperationError>>(command);
        }
    }
}
