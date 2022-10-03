namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationConnectionResponseDTO
    {
        /// <summary>
        /// Describes the access status from KITOS to the organization in the FK Organisation system
        /// </summary>
        public StsOrganizationAccessStatusResponseDTO AccessStatus { get; set; }
    }
}