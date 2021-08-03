using System;

namespace Presentation.Web.Models.API.V1
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
