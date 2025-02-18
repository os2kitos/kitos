using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Notification;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Notifications;
using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications
{
    [RoutePrefix("api/v2/internal/notifications")]
    public class NotificationV2Controller : InternalApiV2Controller
    {
        private readonly INotificationWriteModelMapper _writeModelMapper;
        private readonly INotificationResponseMapper _responseMapper;
        private readonly INotificationService _notificationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public NotificationV2Controller(INotificationWriteModelMapper writeModelMapper,
            INotificationResponseMapper responseMapper,
            INotificationService notificationService,
            IEntityIdentityResolver entityIdentityResolver)
        {
            _writeModelMapper = writeModelMapper;
            _responseMapper = responseMapper;
            _notificationService = notificationService;
            _entityIdentityResolver = entityIdentityResolver;
        }

        /// <summary>
        /// Gets all notifications owned by the specified ownerResourceType with a matching organizationUuid, which become active after the specified fromDate
        /// </summary>
        /// <param name="ownerResourceType">Filter by owner resource type</param>
        /// <param name="organizationUuid">Filter by organization owning the owner resource</param>
        /// <param name="ownerResourceUuid">Filter by uuid of owner resource</param>
        /// <param name="onlyActive">Only include active notifications</param>
        /// <param name="paginationQuery">pagination</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<NotificationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetNotifications(
            OwnerResourceType ownerResourceType,
            [Required][NonEmptyGuid] Guid organizationUuid,
            Guid? ownerResourceUuid = null,
            bool onlyActive = false,
            [FromUri] UnboundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<Advice>>
            {
                new QueryByOwnerResourceType(ownerResourceType.ToRelatedEntityType())
            };

            if (ownerResourceUuid.HasValue)
            {
                var unpackedOwnerResourceUuid = ownerResourceUuid.Value;
                var idResult = ResolveOwnerId(unpackedOwnerResourceUuid, ownerResourceType);
                if (idResult.IsNone)
                    return BadRequest($"OwnerResource with uuid: {unpackedOwnerResourceUuid} was not found");

                conditions.Add(new QueryByOwnerResourceId(idResult.Value));
            }

            if (onlyActive)
            {
                conditions.Add(new QueryByActiveAdvice(true));
            }

            return _notificationService.GetNotifications(organizationUuid, paginationQuery?.Page, paginationQuery?.PageSize, conditions.ToArray())
                .Select(notifications => notifications
                    .Select(_responseMapper.MapNotificationResponseDTO)
                    .ToList())
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Gets a notification based on the ownerResourceType and notificationUuid
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}/{ownerResourceUuid}/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetNotificationByUuid(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _notificationService.GetNotificationByUuid(notificationUuid, ownerResourceUuid, ownerResourceType.ToRelatedEntityType())
                .Select(_responseMapper.MapNotificationResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates an immediate notifications, which is being sent after creation
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{ownerResourceType}/{ownerResourceUuid}/immediate")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult CreateImmediateNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [FromBody] ImmediateNotificationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromImmediatePOST(request, ownerResourceUuid, ownerResourceType);

            return _notificationService.CreateImmediateNotification(parameters)
                .Select(notification => _responseMapper.MapNotificationResponseDTO(notification))
                .Match
                (
                    resultDTO => Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{ownerResourceType}/immediate/{resultDTO}", resultDTO),
                    FromOperationError
                );
        }

        /// <summary>
        /// Creates a scheduled notification. The notification will be sent on a specified date, and will repeat for a specified time
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{ownerResourceType}/{ownerResourceUuid}/scheduled")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult CreateScheduledNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [FromBody] ScheduledNotificationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromScheduledPOST(request, ownerResourceUuid, ownerResourceType);

            return _notificationService.CreateScheduledNotification(parameters)
                .Select(notification => _responseMapper.MapNotificationResponseDTO(notification))
                .Match
                (
                    resultDTO => Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{ownerResourceType}/scheduled/{resultDTO.Uuid}", resultDTO),
                    FromOperationError
                );
        }

        /// <summary>
        /// Updates the scheduled notification
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="notificationUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{ownerResourceType}/{ownerResourceUuid}/scheduled/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult UpdateScheduledNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [NonEmptyGuid] Guid notificationUuid, [FromBody] UpdateScheduledNotificationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var parameters = _writeModelMapper.FromScheduledPUT(request, ownerResourceUuid, ownerResourceType);

            return _notificationService.UpdateScheduledNotification(notificationUuid, parameters)
                .Select(notification => _responseMapper.MapNotificationResponseDTO(notification))
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deactivates the scheduled notification
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{ownerResourceType}/{ownerResourceUuid}/scheduled/deactivate/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeactivateScheduledNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return _notificationService.DeactivateNotification(notificationUuid, ownerResourceUuid, ownerResourceType.ToRelatedEntityType())
                .Select(notification => _responseMapper.MapNotificationResponseDTO(notification))
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{ownerResourceType}/{ownerResourceUuid}/{notificationUuid}")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return _notificationService.DeleteNotification(notificationUuid, ownerResourceUuid, ownerResourceType.ToRelatedEntityType())
                .Match(FromOperationError, NoContent);
        }

        /// <summary>
        /// Gets sent notification information
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}/{ownerResourceUuid}/sent/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<SentNotificationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetSentNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return _notificationService.GetNotificationSentByUuid(notificationUuid, ownerResourceUuid, ownerResourceType.ToRelatedEntityType())
                .Select(x => x.Select(_responseMapper.MapNotificationSentResponseDTO)
                    .ToList()
                )
                .Match(Ok, FromOperationError); 
        }

        /// <summary>
        /// Gets a notification based on the ownerResourceType and notificationUuid
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="ownerResourceUuid"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}/{ownerResourceUuid}/{notificationUuid}/permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NotificationResourcePermissionsDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetPermissions(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid ownerResourceUuid, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _notificationService.GetPermissions(notificationUuid, ownerResourceUuid, ownerResourceType.ToRelatedEntityType())
                .Select(_responseMapper.MapNotificationAccessRightsResponseDTO)
                .Match(Ok, FromOperationError);
        }

        private Maybe<int> ResolveOwnerId(Guid roleUuid, OwnerResourceType relatedEntityType)
        {
            return relatedEntityType switch
            {
                OwnerResourceType.DataProcessingRegistration => _entityIdentityResolver.ResolveDbId<DataProcessingRegistration>(roleUuid),
                OwnerResourceType.ItContract => _entityIdentityResolver.ResolveDbId<ItContract>(roleUuid),
                OwnerResourceType.ItSystemUsage => _entityIdentityResolver.ResolveDbId<ItSystemUsage>(roleUuid),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }
    }
}