using System;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewTechnicalSystemTypeReadModel
    {
        public int Id { get; set; }
        public Guid TechnicalSystemTypeUuid { get; set; }
        public string TechnicalSystemTypeName { get; set; }
        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
