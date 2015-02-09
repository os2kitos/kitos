using System;

namespace Core.DomainModel.ItProject
{
    public class ItProjectPhase : Entity
    {
        /// <summary>
        /// Gets or sets the phase name.
        /// </summary>
        /// <value>
        /// The phase name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the phase start date.
        /// </summary>
        /// <value>
        /// The phase start date.
        /// </value>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Gets or sets the phase end date.
        /// </summary>
        /// <value>
        /// The phase end date.
        /// </value>
        public DateTime? EndDate { get; set; }
        public virtual ItProject ItProject { get; set; }
        public int ItProjectId { get; set; }

    }
}
