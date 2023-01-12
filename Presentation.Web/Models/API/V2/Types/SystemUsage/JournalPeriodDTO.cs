using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Types.SystemUsage
{
    public class JournalPeriodDTO
    {
        [Required]
        public string ArchiveId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public bool Approved { get; set; }
    }
}