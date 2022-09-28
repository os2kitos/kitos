using Core.DomainServices.Model.StsOrganization;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class CheckStsOrganizationConnectionResponse
    {
        public bool Connected { get; }
        public CheckConnectionError? Error { get; }

        public CheckStsOrganizationConnectionResponse(bool connected, CheckConnectionError? connectionErrors = null)
        {
            Connected = connected;
            Error = connectionErrors;
        }
    }
}