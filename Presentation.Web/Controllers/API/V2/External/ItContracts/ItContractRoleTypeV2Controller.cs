using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [Route("api/v2/it-contract-role-types")]
    public class ItContractRoleTypeV2Controller : BaseRoleOptionTypeV2Controller<ItContractRight, ItContractRole>
    {
        public ItContractRoleTypeV2Controller(IOptionsApplicationService<ItContractRight, ItContractRole> optionService, IRoleOptionsApplicationService<ItContractRight, ItContractRole> roleOptionsApplicationService)
            : base(optionService, roleOptionsApplicationService)
        {
        }

        /// <summary>
        /// Returns It Contract role types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the It Contract role availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It Contract role option types</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RoleOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It Contract role type
        /// </summary>
        /// <param name="contractRoleTypeUuid">role type identifier</param>
        /// <param name="organizationUuid">organization context for the role type availability</param>
        /// <returns>A detailed description of the It Contract role type and it's availability</returns>
        [HttpGet]
        [Route("{contractRoleTypeUuid}")]
        [ApiResponse(typeof(RoleOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid contractRoleTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(contractRoleTypeUuid, organizationUuid);
        }
    }
}

