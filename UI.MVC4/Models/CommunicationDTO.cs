using System;

namespace UI.MVC4.Models
{
    public class CommunicationDTO
    {
        public int Id { get; set; }
        public string TargetAudiance { get; set; }
        public string Purpose { get; set; }
        public string Message { get; set; }
        public string Media { get; set; }
        // TODO add json converter
        public DateTime? DueDate { get; set; }

        public int? ResponsibleUserId { get; set; }
        public string ResponsibleUserName { get; set; }
        public int ItProjectId { get; set; }
    }
}