using System;
using System.Net;
using Core.ApplicationServices.Organizations;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    /// <summary>
    /// Internal API for the organizations in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}")]
    public class OrganizationsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IResourcePermissionsResponseMapper _permissionsResponseMapper;

        public OrganizationsInternalV2Controller(IOrganizationService organizationService, IResourcePermissionsResponseMapper permissionsResponseMapper)
        {
            _organizationService = organizationService;
            _permissionsResponseMapper = permissionsResponseMapper;
        }

        [Route("permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourcePermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.GetPermissions(organizationUuid)
                .Select(_permissionsResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }
    }
}