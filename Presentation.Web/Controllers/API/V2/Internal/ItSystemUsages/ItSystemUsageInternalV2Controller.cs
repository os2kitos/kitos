using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.Common.Helpers;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
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
        /// Low-payload search endpoint alternative to the public endpoint which returns full objects.
        /// This one is a convenience endpoint for UI cases returning only
        ///  - Name
        ///  - Uuid
        ///  - Valid state of the local registration
        ///  - Deactivated state of the system master data
        /// </summary>
        /// <param name="organizationUuid">Required organization filter</param>
        /// <param name="relatedToSystemUuid">Query by systems with outgoing relations related to another system</param>
        /// <param name="relatedToSystemUsageUuid">Query by system usages with outgoing relations to a specific system usage (more narrow search than using system id)</param>
        /// <param name="relatedToContractUuid">Query by contracts which are part of a system relation</param>
        /// <param name="systemNameContent">Query usages based on system name</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <returns></returns>
        ///
        [HttpGet]
        [Route("search")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemUsageSearchResultResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetItSystemUsages(
            [NonEmptyGuid] Guid organizationUuid,
            [NonEmptyGuid] Guid? relatedToSystemUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUsageUuid = null,
            [NonEmptyGuid] Guid? relatedToContractUuid = null,
            string systemNameContent = null,
            DateTime? changedSinceGtEq = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .ExecuteItSystemUsagesQuery(organizationUuid, relatedToSystemUuid, relatedToSystemUsageUuid, relatedToContractUuid, null, systemNameContent, changedSinceGtEq, paginationQuery)
                .Select(Map)
                .Transform(Ok);
        }

        /// <summary>
        /// Get roles assigned to the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
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

        private static ItSystemUsageSearchResultResponseDTO Map(ItSystemUsage usage)
        {
            return usage
                .MapIdentityNamePairDTO()
                .Transform(x =>
                    new ItSystemUsageSearchResultResponseDTO(
                        x.Uuid,
                        usage.CheckSystemValidity().Result,
                        usage.ItSystem
                            .MapIdentityNamePairDTO()
                            .Transform(s => new ItSystemUsageSystemContextResponseDTO(s.Uuid, s.Name, usage.ItSystem.Disabled))
                    )
                );
        }
    }
}