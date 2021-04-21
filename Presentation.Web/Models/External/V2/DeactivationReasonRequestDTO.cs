using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2
{
    public class DeactivationReasonRequestDTO
    {
        [Required]
        public string DeactivationReason { get; set; }
    }
}