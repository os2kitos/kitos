using Core.DomainServices.Model.StsOrganization;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class CheckStsOrganizationConnectionResponseDTO
    {
        public bool Connected { get; set; }
        public CheckConnectionError? Error { get; set; }
    }
}