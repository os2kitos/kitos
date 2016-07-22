using System;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// Represents a milestone for a it project. (OIO entity: "Tilstand")
    /// </summary>
    public class Milestone : ItProjectStatus, IProjectModule
    {
        /// <summary>
        /// Gets or sets the date for the milestone.
        /// </summary>
        /// <value>
        /// The milestone date.
        /// </value>
        public DateTime? Date { get; set; }
        /// <summary>
        /// Gets or sets the milestone status.
        /// </summary>
        /// <value>
        /// The milestone status.
        /// </value>
        public TrafficLight Status { get; set; }
    }
}
