using System;

namespace Core.DomainModel.ItProject
{
    public class Communication : Entity
    {
        public string TargetAudiance { get; set; }
        public string Purpose { get; set; }
        public string Message { get; set; }
        public string Media { get; set; }
        public DateTime? DueDate { get; set; }

        public int? ResponsibleUserId { get; set; }
        public virtual User ResponsibleUser { get; set; }
        
        public int ItProjectId { get; set; }
        public virtual ItProject ItProject { get; set; }
    }
}
