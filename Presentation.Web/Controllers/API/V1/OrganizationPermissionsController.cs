using System;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Organizations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/v1/organizations/{organizationUuid}/permissions")]
    public class OrganizationPermissionsController : BaseApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationPermissionsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [Route]
        public HttpResponseMessage Get(Guid organizationUuid)
        {
            return _organizationService
                .CanActiveUserModifyCvr(organizationUuid)
                .Select(canEditCvr => new OrganizationPermissionsDTO { CanEditCvr = canEditCvr })
                .Match(Ok, FromOperationError);
        }
    }
}