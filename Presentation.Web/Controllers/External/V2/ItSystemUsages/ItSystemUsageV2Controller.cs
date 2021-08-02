using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response.SystemUsage;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages
{
    //TODO: To keep it consistent a usage should have uuid. Also it ensures that relations are up to date so that references point in the right direction
    //TODO :Update the itproject and itsystem and dpr apis to allow query by usage id
    /// <summary>
    /// API for the local registrations related to it-systems in KITOS
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// They don't have their own identity ensuring identity-stability across use/discard/use scenarios.
    /// </summary>
    [RoutePrefix("api/v2/it-system-usages")]
    public class ItSystemUsageV2Controller : ExternalBaseController
    {
        /// <summary>
        /// Returns all IT-System usages available to the current user in the specified organization context
        /// </summary>
        /// <param name="organizationUuid">UUID of the</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemUsageResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(
            [NonEmptyGuid] Guid organizationUuid,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns a specific IT-System usage (a specific IT-System in a specific Organization)
        /// </summary>
        /// <param name="systemUsageUuid">UUID of the instance holding the local registrations for a specific IT-System</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystem(
           [NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        //TODO: PUT
        //TODO: POST
        //TODO: DELETE
    }
}