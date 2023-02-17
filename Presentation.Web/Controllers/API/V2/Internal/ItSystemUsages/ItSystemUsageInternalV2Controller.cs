using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages
{
    /// <summary>
    /// Internal API for the local registrations related to it-systems in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/it-system-usages")]
    public class ItSystemUsageInternalV2Controller : InternalApiV2Controller
    {
        private readonly IItSystemUsageService _itSystemUsageService;

        public ItSystemUsageInternalV2Controller(IItSystemUsageService itSystemUsageService)
        {
            _itSystemUsageService = itSystemUsageService;
        }

        /// <summary>
        /// Get roles assigned to the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [InternalApi] //BFF endpoint - internal only
        [HttpGet]
        [Route("{systemUsageUuid}/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExtendedRoleAssignmentResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetAddRoleAssignments([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return _itSystemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Select(x => x.Rights.ToList())
                .Select(rights => rights.Select(right => right.MapExtendedRoleAssignmentResponse()))
                .Match(Ok, FromOperationError);
        }
    }
}