namespace UI.MVC4.Models
{
    public class DataRowUsageDTO
    {
        public int DataRowId { get; set; }
        public int ItSystemUsageId { get; set; }
        public int ItSystemId { get; set; }
        public int ItInterfaceId { get; set; }
        public int InterfaceUsageId { get; set; }
        public int? FrequencyId { get; set; }
        public int? Amount { get; set; }
        public int? Economy { get; set; }
        public int? Price { get; set; }
    }
}
