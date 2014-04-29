namespace Core.DomainModel.ItSystem
{
    public class DataRowUsage : IEntity<int>
    {
        public int Id { get; set; }

        public int InterfaceUsageId { get; set; }
        public virtual InterfaceUsage InterfaceUsage { get; set; }

        public int DataRowId { get; set; }
        public virtual DataRow DataRow { get; set; }


    }
}