using System;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class CommunicationDTO
    {
        public int Id { get; set; }
        public string TargetAudiance { get; set; }
        public string Purpose { get; set; }
        public string Message { get; set; }
        public string Media { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? DueDate { get; set; }

        public int? ResponsibleUserId { get; set; }
        public string ResponsibleUserName { get; set; }
        public int ItProjectId { get; set; }
    }
}