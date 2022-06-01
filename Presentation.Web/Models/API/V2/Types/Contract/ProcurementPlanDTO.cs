using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Types.Contract
{
    public class ProcurementPlanDTO
    {
        /// <summary>
        /// Which half of the year is the procurement plan for.
        /// Range:
        ///     - 1: First quarter of the year
        ///     - 2: Second quarter of the year
        ///     - 3: Third quarter of the year
        ///     - 4: Fourth quarter of the year
        /// </summary>
        [Required]
        [Range(1, 4)]
        public byte QuarterOfYear { get; set; }
        /// <summary>
        /// Which year the procurement plan is for
        /// </summary>
        [Range(0, int.MaxValue)]
        [Required]
        public int Year { get; set; }
    }
}