using Core.ApplicationServices.Notification;
using Core.DomainModel.Notification;
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
        private readonly IUserNotificationService _userNotificationService;

        public UserNotificationController(IUserNotificationService userNotificationService)
        {
            _userNotificationService = userNotificationService;
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Delete(int id)
        {
            return _userNotificationService
                .Delete(id)
                .Match(value => Ok(), FromOperationError);
        }

        [HttpGet]
        [Route("organization/{organizationId}/user/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetByOrganizationAndUser(int organizationId, int userId)
        {
            return _userNotificationService
                .GetNotificationsForUser(organizationId, userId)
                .Match(value => Ok(ToDTOs(value)), FromOperationError);
        }

        [HttpGet]
        [Route("unresolved/organization/{organizationId}/user/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetNumberOfUnresolvedNotifications(int organizationId, int userId)
        {
            return _userNotificationService
                .GetNumberOfUnresolvedNotificationsForUser(organizationId, userId)
                .Match(value => Ok(value), FromOperationError);
        }

        private List<UserNotificationDTO> ToDTOs(IEnumerable<UserNotification> value)
        {
            return value.Select(x => ToDTO(x)).ToList();
        }

        private UserNotificationDTO ToDTO(UserNotification x)
        {
            return new UserNotificationDTO
            {
                Id = x.Id,
                Name = x.Name,
                NotificationMessage = x.NotificationMessage,
                NotificationType = x.NotificationType,
                RelatedEntityId = x.RelatedEntityId,
                RelatedEntityType = x.RelatedEntityType,
                LastChanged = x.LastChanged
            };
        }
    }
}