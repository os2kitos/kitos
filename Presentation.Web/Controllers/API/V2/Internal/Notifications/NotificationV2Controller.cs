using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications
{
    [RoutePrefix("api/v2/notifications")]
    public class NotificationV2Controller : InternalApiV2Controller
    {
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
        public IHttpActionResult GetNotifications(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid organizationUuid, DateTime fromDate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //TODO: call service
            return Ok(new List<NotificationResponseDTO>());
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

            //TODO: call service
            return Ok(new NotificationResponseDTO());
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

            var resultDTO = new NotificationResponseDTO();
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{ownerResourceType}/immediate/{resultDTO.Uuid}", resultDTO);
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

            var resultDTO = new NotificationResponseDTO();
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{ownerResourceType}/scheduled/{resultDTO.Uuid}", resultDTO);
        }

        [HttpPut]
        [Route("{ownerResourceType}/scheduled/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult UpdateScheduledNotification(OwnerResourceType ownerResourceType, [FromBody] UpdateScheduledNotificationWriteRequestDTO request)
        {
            if (!ModelState.IsValid) 
                return BadRequest();

            return Ok(new NotificationResponseDTO());
        }

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

            return Ok(); //TODO: change to NoContent after merging Reference branch to master
        }


        [HttpGet]
        [Route("{ownerResourceType}/{notificationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SentNotificationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetSentNotification(OwnerResourceType ownerResourceType, [NonEmptyGuid] Guid notificationUuid)
        {
            return Ok(new SentNotificationResponseDTO());
        }
    }
}