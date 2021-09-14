using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [RoutePrefix("api/v2/it-contract-role-types")]
    public class ItContractRoleTypeV2Controller : BaseRoleOptionTypeV2Controller<ItContractRight, ItContractRole>
    {
        public ItContractRoleTypeV2Controller(IOptionsApplicationService<ItContractRight, ItContractRole> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It Contract role types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the It Contract role availability</param>
        /// <returns>A list of available It Contract role option types</returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RoleOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RoleOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid contractRoleTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(contractRoleTypeUuid, organizationUuid);
        }
    }
}