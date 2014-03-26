using System;

namespace Core.DomainModel
{
    public class TaskRef : IEntity<int>
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Type { get; set; }
        public string TaskKey { get; set; }
        public string Description { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int? ItProject_Id { get; set; }
        public int? ItSystem_Id { get; set; }

        public virtual ItProject.ItProject ItProject { get; set; }
        public virtual ItSystem.ItSystem ItSystem { get; set; }
    }
}
