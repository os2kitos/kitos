using System;

namespace Presentation.Web.Models.API.V1
{
    public class CommunicationDTO
    {
        public int Id { get; set; }
        public string TargetAudiance { get; set; }
        public string Purpose { get; set; }
        public string Message { get; set; }
        public string Media { get; set; }
        public DateTime? DueDate { get; set; }

        public int? ResponsibleUserId { get; set; }
        public string ResponsibleUserName { get; set; }
        public int ItProjectId { get; set; }
    }
}
