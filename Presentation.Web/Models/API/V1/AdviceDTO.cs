using System;

namespace Presentation.Web.Models.API.V1
{
    public class AdviceDTO
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public DateTime? AlarmDate { get; set; }
        public int? ReceiverId { get; set; }
        public int? CarbonCopyReceiverId { get; set; }
        public string Subject { get; set; }
        public int ItContractId { get; set; }
        public DateTime? SentDate { get; set; }
        public string JobId { get; set; }
    }
}
