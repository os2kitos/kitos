namespace Core.DomainModel.Commands
{
    public class RemoveUserFromOrganizationCommand : ICommand
    {
        public User User { get; }
        public int OrganizationId { get; }

        public RemoveUserFromOrganizationCommand(User user, int organizationId)
        {
            User = user;
            OrganizationId = organizationId;
        }
    }
}
