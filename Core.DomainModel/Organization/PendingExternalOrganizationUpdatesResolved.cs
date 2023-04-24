using Core.DomainModel.Events;

namespace Core.DomainModel.Organization
{
    public class PendingExternalOrganizationUpdatesResolved : IDomainEvent
    {
        public IExternalOrganizationalHierarchyConnection Connection { get; }
        public ExternalConnectionAddNewLogInput Changes { get; }
        public Organization Organization { get; set; }

        public PendingExternalOrganizationUpdatesResolved(Organization organization, IExternalOrganizationalHierarchyConnection connection, ExternalConnectionAddNewLogInput changes)
        {
            Organization = organization;
            Connection = connection;
            Changes = changes;
        }
    }
}
