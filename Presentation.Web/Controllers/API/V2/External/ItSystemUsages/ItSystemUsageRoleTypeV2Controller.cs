using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    [Route("api/v2/it-system-usage-role-types")]
    public class ItSystemUsageRoleTypeV2Controller : BaseRoleOptionTypeV2Controller<ItSystemRight, ItSystemRole>
    {
        public ItSystemUsageRoleTypeV2Controller(IOptionsApplicationService<ItSystemRight, ItSystemRole> optionApplicationService, IRoleOptionsApplicationService<ItSystemRight, ItSystemRole> roleOptionsApplicationService)
            : base(optionApplicationService, roleOptionsApplicationService)
        {
            
        }

        /// <summary>
        /// Returns IT-System usage role types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the IT-System usage role availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available IT-System usage role types</returns>
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

        /// <summary>
        /// Returns requested IT-System usage role type
        /// </summary>
        /// <param name="systemUsageRoleTypeUuid">role type identifier</param>
        /// <param name="organizationUuid">organization context for the role type availability</param>
        /// <returns>A detailed description of the role type and it's availability</returns>
        [HttpGet]
        [Route("{systemUsageRoleTypeUuid}")]
        [ApiResponse(typeof(RoleOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid systemUsageRoleTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(systemUsageRoleTypeUuid, organizationUuid);
        }
    }
}

