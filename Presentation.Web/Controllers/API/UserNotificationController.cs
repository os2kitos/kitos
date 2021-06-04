﻿using Core.ApplicationServices.Notification;
using Core.DomainModel.Advice;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/user-notification")]
    public class UserNotificationController : BaseApiController
    {
        private readonly IUserNotificationApplicationService _userNotificationApplicationService;

        public UserNotificationController(IUserNotificationApplicationService userNotificationApplicationService)
        {
            _userNotificationApplicationService = userNotificationApplicationService;
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Delete(int id)
        {
            return _userNotificationApplicationService
                .Delete(id)
                .Match(value => Ok(), FromOperationError);
        }

        [HttpGet]
        [Route("organization/{organizationId}/context/{relatedEntityType}/user/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetByOrganizationAndUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _userNotificationApplicationService
                .GetNotificationsForUser(organizationId, userId, relatedEntityType)
                .Match(value => Ok(ToDTOs(value)), FromOperationError);
        }

        [HttpGet]
        [Route("unresolved/organization/{organizationId}/context/{relatedEntityType}/user/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetNumberOfUnresolvedNotifications(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _userNotificationApplicationService
                .GetNumberOfUnresolvedNotificationsForUser(organizationId, userId, relatedEntityType)
                .Match(value => Ok(value), FromOperationError);
        }

        private List<UserNotificationDTO> ToDTOs(IEnumerable<UserNotification> value)
        {
            return value.Select(x => ToDTO(x)).ToList();
        }

        private UserNotificationDTO ToDTO(UserNotification x)
        {
            if(x.Itcontract_Id != null)
            {
                return new UserNotificationDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    NotificationMessage = x.NotificationMessage,
                    NotificationType = x.NotificationType,
                    LastChanged = x.LastChanged,
                    RelatedEntityType = RelatedEntityType.itContract,
                    RelatedEntityId = x.Itcontract_Id.Value
                };
            }
            if (x.ItProject_Id != null)
            {
                return new UserNotificationDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    NotificationMessage = x.NotificationMessage,
                    NotificationType = x.NotificationType,
                    LastChanged = x.LastChanged,
                    RelatedEntityType = RelatedEntityType.itProject,
                    RelatedEntityId = x.ItProject_Id.Value
                };
            }
            if (x.ItSystemUsage_Id != null)
            {
                return new UserNotificationDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    NotificationMessage = x.NotificationMessage,
                    NotificationType = x.NotificationType,
                    LastChanged = x.LastChanged,
                    RelatedEntityType = RelatedEntityType.itSystemUsage,
                    RelatedEntityId = x.ItSystemUsage_Id.Value
                };
            }
            if (x.DataProcessingRegistration_Id != null)
            {
                return new UserNotificationDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    NotificationMessage = x.NotificationMessage,
                    NotificationType = x.NotificationType,
                    LastChanged = x.LastChanged,
                    RelatedEntityType = RelatedEntityType.dataProcessingRegistration,
                    RelatedEntityId = x.DataProcessingRegistration_Id.Value
                };
            }
            else
            {
                return null;
            }
        }
    }
}