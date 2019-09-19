using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.System
{
    public class UsingOrganization
    {
        public int ItSystemUsageId { get; }
        public NamedEntity Organization { get; }

        public UsingOrganization(int usageId, NamedEntity organization)
        {
            ItSystemUsageId = usageId;
            Organization = organization;
        }
    }
}
