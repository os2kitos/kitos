using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public partial class ProjectStatus
    {
        public ProjectStatus()
        {
            this.Milestones = new List<Milestone>();
        }

        public int Id { get; set; }

        public virtual ItProject ItProject { get; set; }
        public virtual ICollection<Milestone> Milestones { get; set; }
    }
}
