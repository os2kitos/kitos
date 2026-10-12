namespace Presentation.Web.Models.API.V1.GDPR
{
    public class CreateDataProcessingRegistrationDTO
    {
        public int OrganizationId { get; set; }
        public required string Name { get; set; }
    }
}