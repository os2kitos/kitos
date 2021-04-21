using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2
{
    public class DeactivationReasonRequestDTO
    {
        /// <summary>
        /// Reason for deactivation
        /// </summary>
        [Required]
        public string DeactivationReason { get; set; }
    }
}