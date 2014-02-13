using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public partial class GoalStatus
    {
        public GoalStatus()
        {
            this.Goals = new List<Goal>();
        }

        public int Id { get; set; }

        public virtual ICollection<Goal> Goals { get; set; }
        public virtual ItProject ItProject { get; set; }
    }
}
