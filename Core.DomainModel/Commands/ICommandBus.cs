namespace Core.DomainModel.Commands
{
    public interface ICommandBus
    {
        /// <summary>
        /// Handles the command
        /// </summary>
        /// <param name="args"></param>
        TResult Execute<TCommand,TResult>(TCommand args) where TCommand : ICommand;
    }
}