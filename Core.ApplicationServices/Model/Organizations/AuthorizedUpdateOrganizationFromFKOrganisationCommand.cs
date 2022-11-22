using Core.Abstractions.Types;
using Core.DomainModel.Commands;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Organizations
{
    /// <summary>
    /// Describes a pre-authorized update command for the FK Org synchronization.
    /// Make sure to authorize the call prior to executing this command
    /// </summary>
    public class AuthorizedUpdateOrganizationFromFKOrganisationCommand : ICommand
    {
        public bool SubscribeToChanges { get; }
        public Maybe<int> SynchronizationDepth { get; }
        public Organization Organization { get; }

        public AuthorizedUpdateOrganizationFromFKOrganisationCommand(Organization organization, Maybe<int> synchronizationDepth, bool subscribeToChanges)
        {
            SubscribeToChanges = subscribeToChanges;
            SynchronizationDepth = synchronizationDepth;
            Organization = organization;
        }
    }
}
