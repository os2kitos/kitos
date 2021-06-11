using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2.Request
{
    public class DeactivationReasonRequestDTO
    {
        /// <summary>
        /// Reason for deactivation
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string DeactivationReason { get; set; }
    }
}