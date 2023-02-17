using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Notification;
using Core.DomainModel.Advice;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Notifications;
using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications
{
    [RoutePrefix("api/v2/notifications")]
    public class NotificationV2Controller : InternalApiV2Controller
    {
        private readonly INotificationWriteModelMapper _writeModelMapper;
        private readonly INotificationResponseMapper _responseMapper;
        private readonly INotificationService _notificationService;
        public NotificationV2Controller(INotificationWriteModelMapper writeModelMapper,
            INotificationResponseMapper responseMapper,
            INotificationService notificationService)
        {
            _writeModelMapper = writeModelMapper;
            _responseMapper = responseMapper;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets all notifications owned by the specified ownerResourceType with a matching organizationUuid, which become active after the specified fromDate
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="organizationUuid"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<NotificationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetNotifications(OwnerResourceType ownerResourceType, 
            [NonEmptyGuid] Guid organizationUuid, 
            DateTime? fromDate = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<Advice>>();

            if(fromDate.HasValue)
                conditions.Add(new QueryBySinceNotificationFromDate(fromDate.Value));
            
            return _notificationService.GetNotifications(organizationUuid, conditions.ToArray())
                .Bind
                (
                    notifications =>
                    {
                        var notificationList = notifications
                            .OrderBy(x => x.Id)
                            .Page(paginationQuery)
                            .ToList();

                        return _responseMapper.MapNotificationResponseDTOs(notificationList);
                    }
                )
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Gets a notification based on the ownerResourceType and notificationUuid
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<NotificationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetNotificationByUuid(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _notificationService.GetNotificationByUuid(notificationUuid, ownerResourceType.ToRelatedEntityType())
                .Bind(_responseMapper.MapNotificationResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates an immediate notifications, which is being sent after creation
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{ownerResourceType}/immediate")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult CreateImmediateNotification(OwnerResourceType ownerResourceType, [FromBody] ImmediateNotificationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeModelMapper.FromImmediatePOST(request, ownerResourceType)
                .Bind(notification => _notificationService.CreateImmediateNotification(notification))
                .Bind(notification => _responseMapper.MapNotificationResponseDTO(notification))
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
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{ownerResourceType}/scheduled")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult CreateScheduledNotification(OwnerResourceType ownerResourceType, [FromBody] ScheduledNotificationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeModelMapper.FromScheduledPOST(request, ownerResourceType)
                .Bind(notification => _notificationService.CreateScheduledNotification(notification))
                .Bind(notification => _responseMapper.MapNotificationResponseDTO(notification))
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
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{ownerResourceType}/scheduled/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult UpdateScheduledNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid notificationUuid, [FromBody] UpdateScheduledNotificationWriteRequestDTO request)
        {
            if (!ModelState.IsValid) 
                return BadRequest();

            return _writeModelMapper.FromScheduledPUT(request, ownerResourceType)
                .Bind(notification => _notificationService.UpdateScheduledNotification(notificationUuid, notification))
                .Bind(notification => _responseMapper.MapNotificationResponseDTO(notification))
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deactivates the scheduled notification
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{ownerResourceType}/scheduled/deactivate/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeactivateScheduledNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid) 
                return BadRequest();

            return _notificationService.DeactivateNotification(notificationUuid, ownerResourceType.ToRelatedEntityType())
                .Bind(notification => _responseMapper.MapNotificationResponseDTO(notification))
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{ownerResourceType}/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid notificationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return _notificationService.DeleteNotification(notificationUuid, ownerResourceType.ToRelatedEntityType())
                .Match(FromOperationError, NoContent);
        }

        /// <summary>
        /// Gets sent notification information
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}/sent/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SentNotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetSentNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid notificationUuid)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            return _notificationService.GetNotificationSentByUuid(notificationUuid, ownerResourceType.ToRelatedEntityType())
                .Select(_responseMapper.MapNotificationSentResponseDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Gets a notification based on the ownerResourceType and notificationUuid
        /// </summary>
        /// <param name="ownerResourceType"></param>
        /// <param name="notificationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{ownerResourceType}/access-rights/{notificationUuid}/{ownerResourceUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<NotificationAccessRightsResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetAccessRights(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid notificationUuid, [NonEmptyGuid] Guid ownerResourceUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _notificationService.GetAccessRights(notificationUuid, ownerResourceUuid, ownerResourceType.ToRelatedEntityType())
                .Select(_responseMapper.MapNotificationAccessRightsResponseDTO)
                .Match(Ok, FromOperationError);
        }
    }
}