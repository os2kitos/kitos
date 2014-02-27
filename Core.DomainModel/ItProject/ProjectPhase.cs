using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.DomainModel.ItProject
{
    public class ProjectPhase : IEntity<int>
    {
        public ProjectPhase()
        {
            this.ProjectStatuses = new Collection<ProjectStatus>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }

        public virtual ICollection<ProjectStatus> ProjectStatuses { get; set; }
    }
}