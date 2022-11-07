namespace Core.DomainModel.Commands
{
    public class RemoveUserFromKitosCommand : ICommand
    {
        public User User { get; }

        public RemoveUserFromKitosCommand(User user)
        {
            User = user;
        }
    }
}
