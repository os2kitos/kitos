namespace UI.MVC4.Models
{
    public class InterfaceExposureDTO
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; }
        public int InterfaceId { get; set; }

        public bool IsWishedFor { get; set; }
    }
}