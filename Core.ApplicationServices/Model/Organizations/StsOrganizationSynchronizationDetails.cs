namespace Core.ApplicationServices.Model.Organizations
{
    public class StsOrganizationSynchronizationDetails
    {
        public bool Connected { get; }
        public int? SynchronizationDepth { get; }
        public bool CanCreateConnection { get; }
        public bool CanUpdateConnection { get; }
        public bool CanDeleteConnection { get; }

        public StsOrganizationSynchronizationDetails(bool connected, int? synchronizationDepth, bool canCreateConnection, bool canUpdateConnection, bool canDeleteConnection)
        {
            Connected = connected;
            SynchronizationDepth = synchronizationDepth;
            CanCreateConnection = canCreateConnection;
            CanUpdateConnection = canUpdateConnection;
            CanDeleteConnection = canDeleteConnection;
        }
    }
}
