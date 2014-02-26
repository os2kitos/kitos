using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ProjectStatus
    {
        public ProjectStatus()
        {
            this.Milestones = new List<Milestone>();
        }

        public int Id { get; set; }
        public int ProjectPhase_Id { get; set; }

        public virtual ItProject ItProject { get; set; }
        public virtual ICollection<Milestone> Milestones { get; set; }
        public virtual ProjectPhase ProjectPhase { get; set; }
    }
}
