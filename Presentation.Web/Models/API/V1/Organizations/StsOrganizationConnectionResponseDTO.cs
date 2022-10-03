namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationConnectionResponseDTO
    {
        //TODO: Add "Connected" boolean to indicate if the organization is connected to STS organization
        /// <summary>
        /// Describes the access status from KITOS to the organization in the FK Organisation system
        /// </summary>
        public StsOrganizationAccessStatusResponseDTO AccessStatus { get; set; }
    }
}