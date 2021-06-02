namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewUsedBySystemUsageReadModel : IItSystemUsageOverviewItSystemUsageReadModel
    {
        public int Id { get; set; }

        public int ItSystemUsageId { get; set; }
        public string ItSystemUsageName { get; set; }

        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
