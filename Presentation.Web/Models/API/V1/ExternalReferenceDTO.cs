using System;

namespace Presentation.Web.Models.API.V1
{
    public class ExternalReferenceDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ExternalReferenceId { get; set; }
        public string URL { get; set; }
        public int? ItProject_Id { get; set; }
        public int? ItContract_Id { get; set; }
        public int? ItSystemUsage_Id { get; set; }
        public int? ItSystem_Id { get; set; }
        public int? DataProcessingRegistration_Id;
        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }
        public UserDTO LastChangedByUser { get; set; }
    }
}