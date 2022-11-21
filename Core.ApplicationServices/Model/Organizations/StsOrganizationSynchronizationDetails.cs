using Core.DomainServices.Model.StsOrganization;

namespace Core.ApplicationServices.Model.Organizations
{
    public class StsOrganizationSynchronizationDetails
    {
        public bool Connected { get; }
        public int? SynchronizationDepth { get; }
        public bool CanCreateConnection { get; }
        public bool CanUpdateConnection { get; }
        public bool CanDeleteConnection { get; }
        public CheckConnectionError? CheckConnectionError { get; }
        public bool SubscribesToUpdates { get; }

        public StsOrganizationSynchronizationDetails(bool connected, int? synchronizationDepth, bool canCreateConnection, bool canUpdateConnection, bool canDeleteConnection, CheckConnectionError? checkConnectionError, bool subscribesToUpdates)
        {
            Connected = connected;
            SynchronizationDepth = synchronizationDepth;
            CanCreateConnection = canCreateConnection;
            CanUpdateConnection = canUpdateConnection;
            CanDeleteConnection = canDeleteConnection;
            CheckConnectionError = checkConnectionError;
            SubscribesToUpdates = subscribesToUpdates;
        }
    }
}
