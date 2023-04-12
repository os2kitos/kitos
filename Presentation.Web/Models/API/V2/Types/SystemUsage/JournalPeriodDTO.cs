using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Types.SystemUsage
{
    public class JournalPeriodDTO
    {
        [Required]
        public string ArchiveId { get; set; }
        /// <summary>
        /// Constraint StartDate must be less than or equal to EndDate
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Constraint EndDate must be greater than or equal to StartDate
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public bool Approved { get; set; }
    }
}