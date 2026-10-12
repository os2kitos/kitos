using Core.ApplicationServices.Notification;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Models.API.V2.Types.Notifications;

using System.Net;
namespace Presentation.Web.Controllers.API.V2.Internal.Notifications
{
    [Route("api/v2/internal/alerts")]
    public class AlertsV2Controller : InternalApiV2Controller
    {
        private readonly IUserNotificationApplicationService _userNotificationApplicationService;

        public AlertsV2Controller(IUserNotificationApplicationService userNotificationApplicationService)
        {
            _userNotificationApplicationService = userNotificationApplicationService;
        }

        [HttpDelete]
        [Route("{alertUuid}")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult DeleteAlert([FromRoute][NonEmptyGuid] Guid alertUuid)
        {
            return _userNotificationApplicationService.DeleteByUuid(alertUuid).Match(FromOperationError, NoContent);
        }

        [HttpGet]
        [Route("organization/{organizationUuid}/user/{userUuid}/{ownerResourceType}")]
        [ApiResponse(typeof(IEnumerable<AlertResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetByOrganizationAndUser([FromRoute][NonEmptyGuid] Guid organizationUuid, [FromRoute][NonEmptyGuid] Guid userUuid, OwnerResourceType ownerResourceType)
        {
            var relatedEntityType = ownerResourceType.ToRelatedEntityType();

                return _userNotificationApplicationService.GetNotificationsForUserByUuid(organizationUuid, userUuid, relatedEntityType)
                    .Select(userAlerts => MapUserAlertsToDTO(userAlerts, relatedEntityType))
                    .Match(Ok, FromOperationError);
        }

        private static IEnumerable<AlertResponseDTO> MapUserAlertsToDTO(IQueryable<UserNotification> userAlerts, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.itContract => userAlerts
                    .Select(userAlert => new AlertResponseDTO
                    {
                        AlertType = AlertType.Advis,
                        Created = userAlert.Created,
                        Message = userAlert.NotificationMessage,
                        Name = userAlert.Name,
                        Uuid = userAlert.Uuid,
                        EntityUuid = userAlert.ItContract.Uuid
                    })
                    .ToList(),
                RelatedEntityType.itSystemUsage => userAlerts
                    .Select(userAlert => new AlertResponseDTO
                    {
                        AlertType = AlertType.Advis,
                        Created = userAlert.Created,
                        Message = userAlert.NotificationMessage,
                        Name = userAlert.Name,
                        Uuid = userAlert.Uuid,
                        EntityUuid = userAlert.ItSystemUsage.Uuid
                    })
                    .ToList(),
                RelatedEntityType.dataProcessingRegistration => userAlerts
                    .Select(userAlert => new AlertResponseDTO
                    {
                        AlertType = AlertType.Advis,
                        Created = userAlert.Created,
                        Message = userAlert.NotificationMessage,
                        Name = userAlert.Name,
                        Uuid = userAlert.Uuid,
                        EntityUuid = userAlert.DataProcessingRegistration.Uuid
                    })
                    .ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, "Unsupported related entity type")
            };
        }
    }
}

