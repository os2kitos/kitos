using Core.DomainModel.Events;

namespace Core.DomainModel.Organization
{
    public class PendingExternalOrganizationUpdatesResolved : IDomainEvent
    {
        public ExternalConnectionAddNewLogInput Changes { get; }
        public Organization Organization { get; set; }

        public PendingExternalOrganizationUpdatesResolved(Organization organization, ExternalConnectionAddNewLogInput changes)
        {
            Organization = organization;
            Changes = changes;
        }
    }
}
