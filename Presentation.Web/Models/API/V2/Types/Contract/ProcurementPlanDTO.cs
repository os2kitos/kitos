using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Types.Contract
{
    public class ProcurementPlanDTO
    {
        /// <summary>
        /// Which half of the year is the procurement plan for.
        /// Range:
        ///     - 1: First half of the year
        ///     - 2: Second half of the year
        /// </summary>
        [Required]
        [Range(1, 2)]
        public byte HalfOfYear { get; set; }
        /// <summary>
        /// Which year the procurement plan is for
        /// </summary>
        [Range(0, int.MaxValue)]
        [Required]
        public int Year { get; set; }
    }
}