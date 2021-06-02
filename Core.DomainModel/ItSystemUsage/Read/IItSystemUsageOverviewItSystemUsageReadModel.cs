namespace Core.DomainModel.ItSystemUsage.Read
{
    public interface IItSystemUsageOverviewItSystemUsageReadModel
    {
        public int Id { get; set; }

        public int ItSystemUsageId { get; set; }
        public string ItSystemUsageName { get; set; }

        public int ParentId { get; set; }
        public ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
