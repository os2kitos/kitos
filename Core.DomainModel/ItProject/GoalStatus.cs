using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class GoalStatus : IEntity<int>
    {
        public GoalStatus()
        {
            this.Goals = new List<Goal>();
        }

        public int Id { get; set; }

        public virtual ItProject ItProject { get; set; }

        /// <summary>
        /// Traffic-light dropdown for overall status
        /// </summary>
        public int StatusProject { get; set; }
        /// <summary>
        /// Date-for-status-update field
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// Notes on collected status on project    
        /// </summary>
        public string StatusNote { get; set; }

        public virtual ICollection<Goal> Goals { get; set; }

    }
}
