using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class UpdateSubDataProcessorRequestDTO : ISubDataProcessorRequestDTO
    {
        [Required]
        public int OrganizationId { get; set; }
        [Required]
        public SubDataProcessorDetailsDTO Details { get; set; }
    }
}