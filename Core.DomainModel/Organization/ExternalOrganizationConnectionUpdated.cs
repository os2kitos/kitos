using Core.DomainModel.Events;

namespace Core.DomainModel.Organization
{
    public class ExternalOrganizationConnectionUpdated : EntityUpdatedEvent<Organization>
    {
        public IExternalOrganizationalHierarchyConnection Connection { get; }
        public ExternalConnectionAddNewLogInput Changes { get; }

        public ExternalOrganizationConnectionUpdated(Organization entity, IExternalOrganizationalHierarchyConnection connection, ExternalConnectionAddNewLogInput changes)
            : base(entity)
        {
            Connection = connection;
            Changes = changes;
        }
    }
}
