using Core.DomainModel.ItSystemUsage.GDPR;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewSensitiveDataLevelReadModel
    {
        public int Id { get; set; }
        public SensitiveDataLevel SensitivityDataLevel { get; set; }
        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
