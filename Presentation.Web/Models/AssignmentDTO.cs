using System;

namespace Presentation.Web.Models
{
    public class AssignmentDTO : ItProjectStatusDTO
    {
        /// <summary>
        /// Procentage of activity status
        /// </summary>
        public int StatusProcentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
