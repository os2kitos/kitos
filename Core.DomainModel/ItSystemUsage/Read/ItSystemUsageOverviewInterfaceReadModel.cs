namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewInterfaceReadModel
    {
        public int Id { get; set; }

        public int InterfaceId { get; set; }
        public string InterfaceName { get; set; }

        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
