using System;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Organizations;

namespace Presentation.Web.Controllers.API.V1
{
    /// <summary>
    /// Internal API to control life cycle of organizations
    /// </summary>
    [InternalApi]
    [RoutePrefix("api/v1/organizations")]
    public class OrganizationLifeCycleController : BaseApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationLifeCycleController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [Route("{organizationUuid}/deletion/conflicts")]
        public HttpResponseMessage GetDeletionConflicts(Guid organizationUuid)
        {
            return
            _organizationService
                .ComputeOrganizationRemovalConflicts(organizationUuid)
                .Select(ToConflictsDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{organizationUuid}/deletion")]
        public HttpResponseMessage DeleteOrganization(Guid organizationUuid, bool enforce = false)
        {
            return _organizationService
                .RemoveOrganization(organizationUuid, enforce)
                .Select(FromOperationError)
                .GetValueOrFallback(Ok());
        }

        private static OrganizationRemovalConflictsDTO ToConflictsDTO(OrganizationRemovalConflicts result)
        {
            return new OrganizationRemovalConflictsDTO
            (
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
                );
        }
    }
}