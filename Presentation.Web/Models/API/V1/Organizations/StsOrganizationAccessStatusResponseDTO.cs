using Core.DomainServices.Model.StsOrganization;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationAccessStatusResponseDTO
    {
        /// <summary>
        /// Determines if KITOS has access to data from FK Organisation
        /// </summary>
        public bool AccessGranted { get; set; }
        /// <summary>
        /// If <see cref="AccessGranted"/> is false, this field contains the access error
        /// </summary>
        public CheckConnectionError? Error { get; set; }
    }
}