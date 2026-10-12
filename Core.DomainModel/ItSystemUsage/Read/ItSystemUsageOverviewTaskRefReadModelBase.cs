namespace Core.DomainModel.ItSystemUsage.Read
{
    public abstract class ItSystemUsageOverviewTaskRefReadModelBase
    {
        public int Id { get; set; }
        public string KLEId { get; set; }
        public string KLEName { get; set; }
        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
