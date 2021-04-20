using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External
{
    public class ExternalDeactivationReasonDTO
    {
        [Required]
        public string DeactivationReason { get; set; }
    }
}