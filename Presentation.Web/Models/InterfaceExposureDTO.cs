namespace Presentation.Web.Models
{
    public class InterfaceExposureDTO
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; }
        public ItSystemUsageSimpleDTO ItSystemUsage { get; set; }

        public ItSystemDTO Interface { get; set; }

        public int? ItContractId { get; set; }
        public ItContractSystemDTO ItContract { get; set; }

        public bool IsWishedFor { get; set; }
    }
}
