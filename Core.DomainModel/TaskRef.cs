using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class TaskRef : Entity
    {
        public TaskRef()
        {
            this.Children = new List<TaskRef>();
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.ItSystemUsages = new List<ItSystemUsage>();
            this.ItProjects = new List<ItProject.ItProject>();
        }
        
        public bool IsPublic { get; set; }
        public Guid Uuid { get; set; }
        public string Type { get; set; }
        public string TaskKey { get; set; }
        public string Description { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int? ItProjectId { get; set; }
        public int? ParentId { get; set; }
        public int OwnedByOrganizationUnitId { get; set; }

        public virtual TaskRef Parent { get; set; }
        public virtual ICollection<TaskRef> Children { get; set; }
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }
        public virtual ICollection<TaskUsage> Usages { get; set; }
        public virtual OrganizationUnit OwnedByOrganizationUnit { get; set; }
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }
        public virtual ICollection<ItSystemUsage> ItSystemUsages { get; set; }
    }
}
