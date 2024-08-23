using Core.ApplicationServices.Organizations;
using System.Web.Http;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    /// <summary>
    /// Internal API for the local registrations related to it-systems in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations")]
    public class OrganizationsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationService _organizationService;
    }
}