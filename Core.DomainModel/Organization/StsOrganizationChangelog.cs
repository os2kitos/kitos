using System;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationChangeLog
    {
        public string ResponsibleEntityName { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public ConnectionUpdateOrganizationUnitChangeType Type { get; set; }
        public string Description { get; set; }
    }
}
