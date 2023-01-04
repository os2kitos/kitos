using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class AssignSubDataProcessorRequestDTO : ISubDataProcessorRequestDTO
    {
        [Required]
        public int OrganizationId { get; set; }
        public SubDataProcessorDetailsDTO Details { get; set; }
    }
}