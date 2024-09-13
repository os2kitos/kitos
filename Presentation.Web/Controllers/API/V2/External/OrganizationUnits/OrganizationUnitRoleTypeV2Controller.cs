using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.OrganizationUnits
{
    [RoutePrefix("api/v2/organization-unit-role-types")]
    public class OrganizationUnitRoleTypeV2Controller : BaseRoleOptionTypeV2Controller<OrganizationUnitRight, OrganizationUnitRole>
    {
        public OrganizationUnitRoleTypeV2Controller(IOptionsApplicationService<OrganizationUnitRight, OrganizationUnitRole> optionApplicationService)
            : base(optionApplicationService)
        {

        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RoleOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        [HttpGet]
        [Route("{organizationUnitRoleTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RoleOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUnitRoleTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(organizationUnitRoleTypeUuid, organizationUuid);
        }
    }
}