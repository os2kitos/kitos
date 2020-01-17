using System;

namespace Presentation.Web.Models
{
    public class TaskRefDTO
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public Guid Uuid { get; set; }
        public string Type { get; set; }
        public string TaskKey { get; set; }
        public string Description { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int OwnedByOrganizationUnitId { get; set; }
        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }
    }
}