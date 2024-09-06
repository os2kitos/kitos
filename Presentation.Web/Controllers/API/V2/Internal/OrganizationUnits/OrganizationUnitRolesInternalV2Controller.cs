using System;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits
{
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/organization-units/{organizationUnitUuid}/roles")]
    public class OrganizationUnitRolesInternalV2Controller : InternalApiV2Controller
    {
        [Route("get")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUnitRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetRoles([NonEmptyGuid] Guid orgUuid, [NonEmptyGuid] Guid unitUuid)
        {
            return Ok();
        }

        [Route("create")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(OrganizationUnitRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult CreateRole([NonEmptyGuid] Guid orgUuid, [NonEmptyGuid] Guid unitUuid, [FromBody] CreateOrganizationUnitRoleRequestDTO parameters)
        {
            return Ok();
        }

        [Route("patch")]
        [HttpPatch]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUnitRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PatchRole([NonEmptyGuid] Guid orgUuid, [NonEmptyGuid] Guid unitUuid, [FromBody] UpdateOrganizationUnitRoleRequestDTO parameters)
        {
            return Ok();
        }

        [Route("delete")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteRole([NonEmptyGuid] Guid orgUuid, [NonEmptyGuid] Guid unitUuid)
        {
            return Ok();
        }
    }
}