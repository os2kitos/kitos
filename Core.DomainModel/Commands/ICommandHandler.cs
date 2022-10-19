namespace Core.DomainModel.Commands
{
    public interface ICommandHandler<in TCommand, out TResult> where TCommand:ICommand
    {
        TResult Execute(TCommand command);
    }
}