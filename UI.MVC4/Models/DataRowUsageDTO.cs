namespace UI.MVC4.Models
{
    public class DataRowUsageDTO
    {
        public int Id { get; set; }

        public int InterfaceUsageId { get; set; }
        public int DataRowId { get; set; }

        public int? FrequencyId { get; set; }

        public int Amount { get; set; }
        public int Economy { get; set; }
        public int Price { get; set; }

    }
}