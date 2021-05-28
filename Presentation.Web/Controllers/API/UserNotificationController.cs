using Core.ApplicationServices.Notification;
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
    }
}