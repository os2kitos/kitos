using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Request.SystemUsage;
using Presentation.Web.Models.External.V2.Response.SystemUsage;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages
{
    /// <summary>
    /// API for the local registrations related to it-systems in KITOS
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// </summary>
    [RoutePrefix("api/v2/it-system-usages")]
    public class ItSystemUsageV2Controller : ExternalBaseController
    {
        /// <summary>
        /// Returns all IT-System usages available to the current user in the specified organization context
        /// </summary>
        /// <param name="organizationUuid">Query usages within a specific organization</param>
        /// <param name="relatedToSystemUuid">Query by systems related to another system</param>
        /// <param name="relatedToSystemUsageUuid">Query by system usages related to a specific system usage</param>
        /// <param name="systemUuid">Query usages of a specific system</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemUsageResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemUsages(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUsageUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            string systemNameContent = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns a specific IT-System usage (a specific IT-System in a specific Organization)
        /// </summary>
        /// <param name="systemUsageUuid">UUID of the system usage entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Deletes a system usage.
        /// NOTE: this action also clears any incoming relation e.g. relations from other system usages, contracts, projects or data processing registrations.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates an IT-System usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict,description:"Another system usage has already been created for the system within the specified organization")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult CreateItSystemUsage([FromBody]CreateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the properties of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult UpdateSystemUsage([FromBody] UpdateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        //TODO: Relations
    }
}