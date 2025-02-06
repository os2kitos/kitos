using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Generic;
using Presentation.Web.Controllers.API.V2.Common.Helpers;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
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
        private readonly IItSystemUsageWriteService _writeService;
        private readonly IItsystemUsageRelationsService _relationsService;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IItSystemUsageResponseMapper _responseMapper;

        public ItSystemUsageInternalV2Controller(IItSystemUsageService itSystemUsageService, IItSystemUsageWriteService writeService, IItsystemUsageRelationsService relationsService, IEntityIdentityResolver identityResolver, IItSystemUsageResponseMapper responseMapper)
        {
            _itSystemUsageService = itSystemUsageService;
            _writeService = writeService;
            _relationsService = relationsService;
            _identityResolver = identityResolver;
            _responseMapper = responseMapper;
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
        /// <param name="orderByProperty">Ordering property</param>
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
            [NonEmptyGuid] Guid? systemUuid = null,
            string systemNameContent = null,
            DateTime? changedSinceGtEq = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .ExecuteItSystemUsagesQuery(organizationUuid, relatedToSystemUuid, relatedToSystemUsageUuid, relatedToContractUuid, systemUuid, systemNameContent, changedSinceGtEq, orderByProperty, paginationQuery)
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

        /// <summary>
        /// Deletes a system usage by organizationUuid and systemUuid.
        /// </summary>
        /// <param name="organizationUuid"></param>
        /// <param name="systemUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("system/{systemUuid}/organization/{organizationUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteItSystemUsageByOrganizationUuidAndSystemUuid([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid systemUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteByItSystemAndOrganizationUuids(systemUuid, organizationUuid)
                .Match(FromOperationError, () => StatusCode(HttpStatusCode.NoContent));
        }

        [HttpGet]
        [Route("relations/{contractUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<GeneralSystemRelationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetRelations([NonEmptyGuid] Guid contractUuid)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var contractId = _identityResolver.ResolveDbId<ItContract>(contractUuid);
            if (contractId.IsNone)
            {
                return FromOperationError(new OperationError($"Contract with uuid was not found: {contractUuid}",
                    OperationFailure.NotFound));
            }

            return _relationsService.GetRelationsAssociatedWithContract(contractId.Value)
                .Select(relations => relations.Select(_responseMapper.MapGeneralSystemRelationDTO))
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