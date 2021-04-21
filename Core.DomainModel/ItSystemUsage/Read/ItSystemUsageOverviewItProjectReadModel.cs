namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewItProjectReadModel
    {
        public int Id { get; set; }

        public int ItProjectId { get; set; }
        public string ItProjectName { get; set; }

        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
