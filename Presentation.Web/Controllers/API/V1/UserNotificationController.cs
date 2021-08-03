using Core.ApplicationServices.Notification;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Models.API.V1.UserNotification;

namespace Presentation.Web.Controllers.API.V1
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
                .Match(FromOperationError, Ok);
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
                .Select(x => x.OrderBy(y => y.Created))
                .Match(value => Ok(ToDTOs(value)), FromOperationError);
        }

        [HttpGet]
        [Route("unresolved/organization/{organizationId}/context/{relatedEntityType}/user/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public HttpResponseMessage GetNumberOfUnresolvedNotifications(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _userNotificationApplicationService
                .GetNotificationsForUser(organizationId, userId, relatedEntityType)
                .Select(x => x.Count())
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
                Created = x.Created,
                RelatedEntityType = x.GetRelatedEntityType(),
                RelatedEntityId = x.GetRelatedEntityId()
            };
        }
    }
}