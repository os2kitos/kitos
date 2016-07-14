using System;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    ///
    /// </summary>
    public class Assignment : ItProjectStatus
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Procentage of assignment completion.
        /// </summary>
        public int StatusProcentage { get; set; }
    }
}
