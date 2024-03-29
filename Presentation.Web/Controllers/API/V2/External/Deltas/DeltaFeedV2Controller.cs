﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.Tracking;
using Core.DomainModel.Tracking;
using Presentation.Web.Controllers.API.V2.External.Deltas.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Tracking;
using Presentation.Web.Models.API.V2.Types.Shared;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.Deltas
{
    [RoutePrefix("api/v2/delta-feed")]
    public class DeltaFeedV2Controller : ExternalBaseController
    {
        private readonly ITrackingService _trackingService;

        public DeltaFeedV2Controller(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        /// <summary>
        /// Returns a feed of deleted items, optionally since a specified time (UTC)
        /// </summary>
        /// <param name="entityType">Filter results based on tracked entity type.</param>
        /// <param name="deletedSinceUTC">Results will be returned where 'deletedTimeStamp >= deletedSinceUTC'</param>
        /// <returns></returns>
        [HttpGet]
        [Route("deleted-entities")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<TrackingEventResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetDeletedObjects(
            TrackedEntityTypeChoice? entityType = null,
            DateTime? deletedSinceUTC = null,
            [FromUri] BoundedPaginationQuery pagination = null)
        {
            var dtos = _trackingService
                .QueryLifeCycleEvents(TrackedLifeCycleEventType.Deleted, entityType?.ToDomainType(), deletedSinceUTC)
                .OrderBy(x => x.OccurredAtUtc)
                .Page(pagination)
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