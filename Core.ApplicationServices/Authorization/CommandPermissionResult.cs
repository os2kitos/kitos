namespace Core.ApplicationServices.Authorization
{
    public class CommandPermissionResult
    {
        public CommandPermissionResult(string id, bool canExecute)
        {
            Id = id;
            CanExecute = canExecute;
        }

        public string Id { get; }
        public bool CanExecute { get; }
    }
}
