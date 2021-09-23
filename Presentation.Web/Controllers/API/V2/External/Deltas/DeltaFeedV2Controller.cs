using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Tracking;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Presentation.Web.Controllers.API.V2.External.Deltas.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Tracking;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.Deltas
{
    [RoutePrefix("api/v2/delta-feed")]
    public class DeltaFeedV2Controller : ExternalBaseController
    {
        private readonly IGenericRepository<LifeCycleTrackingEvent> _trackingEventsRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;

        public DeltaFeedV2Controller(IGenericRepository<LifeCycleTrackingEvent> trackingEventsRepository, IAuthorizationContext authorizationContext, IOrganizationalUserContext userContext)
        {
            _trackingEventsRepository = trackingEventsRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
        }

        /// <summary>
        /// Returns a feed of deleted items, optionally since a specified time (UTC)
        /// </summary>
        /// <param name="deletedSinceUTC">Query by KLE category</param>
        /// <returns></returns>
        [HttpGet]
        [Route("deleted-entities")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<TrackingEventResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetDeletedObjects(
            DateTime? deletedSinceUTC = null,
            [FromUri] BoundedPaginationQuery pagination = null)
        {
            //TODO: Move stuff to a service once finished

            var query = _trackingEventsRepository
                .AsQueryable()
                .Where(x => x.EventType == TrackedLifeCycleEventType.Deleted);

            var accessLevel = _authorizationContext.GetCrossOrganizationReadAccess();

            if (accessLevel < CrossOrganizationDataReadAccessLevel.All)
            {
                var organizationIds = _userContext.OrganizationIds.ToList();

                if (accessLevel == CrossOrganizationDataReadAccessLevel.RightsHolder)
                {
                    //TODO: Organization id's where rightsholder - how to solve that?
                    throw new NotSupportedException();
                }
                else if (accessLevel == CrossOrganizationDataReadAccessLevel.Public)
                {
                    query = query.Where(x =>
                        x.OptionalOrganizationReference == null ||
                        organizationIds.Contains(x.OptionalOrganizationReference.Id) ||
                        x.OptionalAccessModifier == AccessModifier.Public);
                }
                else
                {
                    query = query.Where(x =>
                        x.OptionalOrganizationReference == null ||
                        organizationIds.Contains(x.OptionalOrganizationReference.Id)
                    );
                }
            }

            if (deletedSinceUTC.HasValue)
            {
                var occurredAt = deletedSinceUTC.Value.ToUniversalTime();
                query = query.Where(x => x.OccurredAtUtc >= occurredAt);
            }

            query = query
                .OrderBy(x => x.OccurredAtUtc)
                .Page(pagination);

            var dtos = query
                .AsNoTracking()
                .AsEnumerable()
                .Select(ToDTO)
                .ToList();

            return Ok(dtos);
        }

        private static TrackingEventResponseDTO ToDTO(LifeCycleTrackingEvent arg)
        {
            return new TrackingEventResponseDTO
            {
                EntityType = arg.EntityType.ToApiType(),
                EntityUuid = arg.EntityUuid,
                OccurredAtUtc = DateTime.SpecifyKind(arg.OccurredAtUtc, DateTimeKind.Utc)
            };
        }
    }
}