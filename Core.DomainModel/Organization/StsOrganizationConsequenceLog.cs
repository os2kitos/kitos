using System;

namespace Core.DomainModel.Organization
{
    public class StsOrganizationConsequenceLog : Entity
    {
        public int ChangeLogId { get; set; }
        public StsOrganizationChangeLog ChangeLog { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public ConnectionUpdateOrganizationUnitChangeType Type { get; set; }
        public string Description { get; set; }
    }
}
