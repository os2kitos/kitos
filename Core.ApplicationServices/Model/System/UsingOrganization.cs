using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.System
{
    public class UsingOrganization
    {
        public NamedEntityWithUuid ItSystemUsage { get; set; }
        public NamedEntityWithUuid Organization { get; }

        public UsingOrganization(NamedEntityWithUuid itSystemUsage, NamedEntityWithUuid organization)
        {
            ItSystemUsage = itSystemUsage;
            Organization = organization;
        }
    }
}
