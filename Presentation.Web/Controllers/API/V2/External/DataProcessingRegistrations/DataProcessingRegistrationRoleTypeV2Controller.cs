using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations
{
    [RoutePrefix("api/v2/data-processing-registrations-role-types")]
    public class DataProcessingRegistrationRoleTypeV2Controller : BaseRoleOptionTypeV2Controller<DataProcessingRegistrationRight, DataProcessingRegistrationRole>
    {
        public DataProcessingRegistrationRoleTypeV2Controller(IOptionsApplicationService<DataProcessingRegistrationRight, DataProcessingRegistrationRole> optionApplicationService)
            : base(optionApplicationService)
        {
            
        }

        /// <summary>
        /// Returns Data Processing Registration role types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the Data Processing Registration role availability</param>
        /// <returns>A list of available Data Processing Registration role option types</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RoleOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested Data Processing Registration role type
        /// </summary>
        /// <param name="dataProcessingRegistrationRoleTypeUuid">role type identifier</param>
        /// <param name="organizationUuid">organization context for the role type availability</param>
        /// <returns>A detailed description of the Data Processing Registration role type and it's availability</returns>
        [HttpGet]
        [Route("{dataProcessingRegistrationRoleTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RoleOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid dataProcessingRegistrationRoleTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(dataProcessingRegistrationRoleTypeUuid, organizationUuid);
        }
    }
}