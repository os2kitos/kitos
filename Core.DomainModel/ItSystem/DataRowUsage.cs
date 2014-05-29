namespace Core.DomainModel.ItSystem
{
    public class DataRowUsage : Entity
    {
        public int InterfaceUsageId { get; set; }
        public virtual InterfaceUsage InterfaceUsage { get; set; }

        public int DataRowId { get; set; }
        public virtual DataRow DataRow { get; set; }

        public int? FrequencyId { get; set; }
        public virtual Frequency Frequency { get; set; }

        public int Amount { get; set; }
        public int Economy { get; set; }
        public int Price { get; set; }
    }
}