using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Presentation.Web.Controllers.API.V2.External.OrganizationUnits
{
    [Route("api/v2/organization-unit-role-types")]
    public class OrganizationUnitRoleTypeV2Controller : BaseRoleOptionTypeV2Controller<OrganizationUnitRight, OrganizationUnitRole>
    {
        public OrganizationUnitRoleTypeV2Controller(IOptionsApplicationService<OrganizationUnitRight, OrganizationUnitRole> optionApplicationService, IRoleOptionsApplicationService<OrganizationUnitRight, OrganizationUnitRole> roleOptionsApplicationService)
            : base(optionApplicationService, roleOptionsApplicationService)
        {

        }

        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RoleOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        [HttpGet]
        [Route("{organizationUnitRoleTypeUuid}")]
        [ApiResponse(typeof(RoleOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUnitRoleTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(organizationUnitRoleTypeUuid, organizationUuid);
        }
    }
}


